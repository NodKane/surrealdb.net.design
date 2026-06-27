function Invoke-SurrealDbDesignTool {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string[]] $Arguments,

        [string] $Project
    )

    $toolPath = Join-Path $PSScriptRoot "..\lib\net10.0\SurrealDb.Net.Design.dll"
    if (-not (Test-Path $toolPath)) {
        $toolPath = Join-Path $PSScriptRoot "net10.0\any\SurrealDb.Net.Design.dll"
    }

    if (-not (Test-Path $toolPath)) {
        throw "SurrealDb.Net.Design tool assembly was not found at '$toolPath'. Restore or reinstall the package."
    }

    $projectDirectory = $null
    $getProjectCommand = Get-Command Get-Project -ErrorAction SilentlyContinue
    if ($getProjectCommand) {
        try {
            $projectObject = if ([string]::IsNullOrWhiteSpace($Project)) {
                Get-Project
            }
            else {
                Get-Project -Name $Project
            }

            if ($projectObject -and $projectObject.FullName) {
                $projectDirectory = Split-Path -Parent $projectObject.FullName
            }
        }
        catch {
            if (-not [string]::IsNullOrWhiteSpace($Project)) {
                throw
            }
        }
    }

    $locationWasChanged = $false
    if (-not [string]::IsNullOrWhiteSpace($projectDirectory) -and (Test-Path $projectDirectory)) {
        Push-Location $projectDirectory
        $locationWasChanged = $true
    }

    try {
        & dotnet $toolPath @Arguments
        if ($LASTEXITCODE -ne 0) {
            throw "SurrealDb.Net.Design exited with code $LASTEXITCODE."
        }
    }
    finally {
        if ($locationWasChanged) {
            Pop-Location
        }
    }
}

function Scaffold-SurrealDbDatabase {
    [CmdletBinding()]
    param(
        [string] $Connection,
        [string] $Endpoint,
        [string] $Namespace,
        [string] $Database,
        [string] $User,
        [string] $Password,
        [string] $Token,
        [string] $Output,
        [string] $ModelNamespace,
        [string] $Context,
        [string] $ContextNamespace,
        [switch] $NoContext,
        [string] $RecordBaseType,
        [string] $RecordNamespace,
        [string[]] $Table,
        [string] $SchemaFile,
        [switch] $Overwrite,
        [string] $Project
    )

    $arguments = New-Object System.Collections.Generic.List[string]
    $arguments.Add("scaffold")
    $arguments.Add("db")

    if (-not [string]::IsNullOrWhiteSpace($Connection)) { $arguments.Add("--connection"); $arguments.Add($Connection) }
    if (-not [string]::IsNullOrWhiteSpace($Endpoint)) { $arguments.Add("--endpoint"); $arguments.Add($Endpoint) }
    if (-not [string]::IsNullOrWhiteSpace($Namespace)) { $arguments.Add("--namespace"); $arguments.Add($Namespace) }
    if (-not [string]::IsNullOrWhiteSpace($Database)) { $arguments.Add("--database"); $arguments.Add($Database) }
    if (-not [string]::IsNullOrWhiteSpace($User)) { $arguments.Add("--user"); $arguments.Add($User) }
    if (-not [string]::IsNullOrWhiteSpace($Password)) { $arguments.Add("--password"); $arguments.Add($Password) }
    if (-not [string]::IsNullOrWhiteSpace($Token)) { $arguments.Add("--token"); $arguments.Add($Token) }
    if (-not [string]::IsNullOrWhiteSpace($Output)) { $arguments.Add("--output"); $arguments.Add($Output) }
    if (-not [string]::IsNullOrWhiteSpace($ModelNamespace)) { $arguments.Add("--model-namespace"); $arguments.Add($ModelNamespace) }
    if (-not [string]::IsNullOrWhiteSpace($Context)) { $arguments.Add("--context"); $arguments.Add($Context) }
    if (-not [string]::IsNullOrWhiteSpace($ContextNamespace)) { $arguments.Add("--context-namespace"); $arguments.Add($ContextNamespace) }
    if ($NoContext) { $arguments.Add("--no-context") }
    if (-not [string]::IsNullOrWhiteSpace($RecordBaseType)) { $arguments.Add("--record-base-type"); $arguments.Add($RecordBaseType) }
    if (-not [string]::IsNullOrWhiteSpace($RecordNamespace)) { $arguments.Add("--record-namespace"); $arguments.Add($RecordNamespace) }
    if ($Table) {
        foreach ($tableName in $Table) {
            if (-not [string]::IsNullOrWhiteSpace($tableName)) {
                $arguments.Add("--table")
                $arguments.Add($tableName)
            }
        }
    }
    if (-not [string]::IsNullOrWhiteSpace($SchemaFile)) { $arguments.Add("--schema-file"); $arguments.Add($SchemaFile) }
    if ($Overwrite) { $arguments.Add("--overwrite") }

    Invoke-SurrealDbDesignTool -Arguments $arguments.ToArray() -Project $Project
}

Set-Alias -Name Scaffold-SurrealDbContext -Value Scaffold-SurrealDbDatabase -Scope Global
