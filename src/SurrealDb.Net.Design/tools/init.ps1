. (Join-Path $PSScriptRoot "SurrealDb.Net.Design.Commands.ps1")

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

    Invoke-SurrealDbScaffoldDatabase @PSBoundParameters
}

Set-Alias -Name Scaffold-SurrealDbContext -Value Scaffold-SurrealDbDatabase -Scope Global
