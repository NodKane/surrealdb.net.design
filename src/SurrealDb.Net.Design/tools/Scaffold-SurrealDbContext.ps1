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

. (Join-Path $PSScriptRoot "SurrealDb.Net.Design.Commands.ps1")
Invoke-SurrealDbScaffoldDatabase @PSBoundParameters
