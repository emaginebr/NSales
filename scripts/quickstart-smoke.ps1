#requires -Version 7.0
<#
    Quickstart smoke test for feature 002-category-subcategories.
    Automates sections 4-7 of specs/002-category-subcategories/quickstart.md.

    Exercises both surfaces depending on the chosen tenant's Marketplace flag.

    Usage:
        ./scripts/quickstart-smoke.ps1 -BaseUrl https://localhost:44374 -Tenant emagine -Token "$env:LOFN_TOKEN" -Marketplace
        ./scripts/quickstart-smoke.ps1 -BaseUrl https://localhost:44374 -Tenant monexup -Token "$env:LOFN_TOKEN" -StoreSlug minha-loja

    Exit codes:
        0  all assertions passed
        1  one or more assertions failed
#>

param(
    [Parameter(Mandatory)] [string] $BaseUrl,
    [Parameter(Mandatory)] [string] $Tenant,
    [Parameter(Mandatory)] [string] $Token,
    [string] $StoreSlug,
    [switch] $Marketplace
)

$ErrorActionPreference = 'Stop'

# Disable TLS validation (dev cert).
[System.Net.ServicePointManager]::ServerCertificateValidationCallback = { $true }

if (-not $Marketplace -and -not $StoreSlug) {
    Write-Error "Non-marketplace mode requires -StoreSlug."
    exit 1
}

$failed = 0
$passed = 0
$insertedIds = @()

function Invoke-Lofn {
    param(
        [Parameter(Mandatory)] [string] $Method,
        [Parameter(Mandatory)] [string] $Path,
        $Body,
        [switch] $Anonymous
    )

    $headers = @{
        'X-Tenant-Id'  = $Tenant
        'Content-Type' = 'application/json'
    }
    if (-not $Anonymous) { $headers['Authorization'] = "Bearer $Token" }

    $args = @{
        Uri              = "$BaseUrl$Path"
        Method           = $Method
        Headers          = $headers
        SkipHttpErrorCheck = $true
        ContentType      = 'application/json'
    }
    if ($null -ne $Body) {
        $args.Body = ($Body | ConvertTo-Json -Compress -Depth 6)
    }

    $response = Invoke-WebRequest @args
    return [pscustomobject]@{
        StatusCode = [int]$response.StatusCode
        Body       = if ($response.Content) { $response.Content } else { '' }
    }
}

function Assert {
    param(
        [Parameter(Mandatory)] [string] $Description,
        [Parameter(Mandatory)] [bool]   $Condition,
        [string] $Detail
    )
    if ($Condition) {
        $script:passed++
        Write-Host "  ok   - $Description" -ForegroundColor Green
    } else {
        $script:failed++
        Write-Host "  FAIL - $Description" -ForegroundColor Red
        if ($Detail) { Write-Host "         $Detail" -ForegroundColor DarkGray }
    }
}

function Insert-Category {
    param([string] $Name, [Nullable[long]] $ParentId)

    $body = @{ name = $Name }
    if ($null -ne $ParentId) { $body.parentCategoryId = $ParentId }

    if ($Marketplace) {
        $r = Invoke-Lofn -Method POST -Path '/category-global/insert' -Body $body
    } else {
        $r = Invoke-Lofn -Method POST -Path "/category/$StoreSlug/insert" -Body $body
    }
    return $r
}

function Update-Category {
    param([Parameter(Mandatory)] [long] $CategoryId, [string] $Name, [Nullable[long]] $ParentId)

    $body = @{ categoryId = $CategoryId; name = $Name }
    if ($null -ne $ParentId) { $body.parentCategoryId = $ParentId }

    if ($Marketplace) {
        return Invoke-Lofn -Method POST -Path '/category-global/update' -Body $body
    }
    return Invoke-Lofn -Method POST -Path "/category/$StoreSlug/update" -Body $body
}

function Delete-Category {
    param([Parameter(Mandatory)] [long] $CategoryId)

    if ($Marketplace) {
        return Invoke-Lofn -Method DELETE -Path "/category-global/delete/$CategoryId"
    }
    return Invoke-Lofn -Method DELETE -Path "/category/$StoreSlug/delete/$CategoryId"
}

function Cleanup {
    foreach ($id in ($insertedIds | Sort-Object -Descending)) {
        try { Delete-Category -CategoryId $id | Out-Null } catch { }
    }
}

trap { Cleanup; throw }

try {
    Write-Host "`n== §4. Three-level hierarchy ==" -ForegroundColor Cyan

    $rootName = "QSRoot-$([guid]::NewGuid().ToString('N').Substring(0,8))"
    $r = Insert-Category -Name $rootName
    Assert -Description "§4.1 insert root succeeds (HTTP 2xx)" -Condition ($r.StatusCode -ge 200 -and $r.StatusCode -lt 300) -Detail "status=$($r.StatusCode) body=$($r.Body.Substring(0,[Math]::Min(200,$r.Body.Length)))"
    $rootJson = $r.Body | ConvertFrom-Json
    $insertedIds += $rootJson.categoryId
    Assert "§4.1 root parentCategoryId is null" ($null -eq $rootJson.parentCategoryId)
    Assert "§4.1 root slug is single-segment (no '/')" ($rootJson.slug -notmatch '/')

    $childName = "QSChild-$([guid]::NewGuid().ToString('N').Substring(0,8))"
    $r = Insert-Category -Name $childName -ParentId $rootJson.categoryId
    Assert "§4.2 insert child succeeds" ($r.StatusCode -ge 200 -and $r.StatusCode -lt 300)
    $childJson = $r.Body | ConvertFrom-Json
    $insertedIds += $childJson.categoryId
    Assert "§4.2 child parentCategoryId == root" ($childJson.parentCategoryId -eq $rootJson.categoryId)
    Assert "§4.2 child slug starts with '$($rootJson.slug)/'" ($childJson.slug.StartsWith("$($rootJson.slug)/"))

    $grandName = "QSGrand-$([guid]::NewGuid().ToString('N').Substring(0,8))"
    $r = Insert-Category -Name $grandName -ParentId $childJson.categoryId
    Assert "§4.3 insert grandchild succeeds" ($r.StatusCode -ge 200 -and $r.StatusCode -lt 300)
    $grandJson = $r.Body | ConvertFrom-Json
    $insertedIds += $grandJson.categoryId
    Assert "§4.3 grandchild slug starts with '$($childJson.slug)/'" ($grandJson.slug.StartsWith("$($childJson.slug)/"))

    Write-Host "`n== §4.4 GraphQL tree ==" -ForegroundColor Cyan
    $treeQuery = if ($Marketplace) {
        '{ categoryTree { categoryId name slug children { categoryId name slug children { categoryId name slug } } } }'
    } else {
        "{ categoryTree(storeSlug: \`"$StoreSlug\`") { categoryId name slug children { categoryId name slug children { categoryId name slug } } } }"
    }
    $r = Invoke-Lofn -Method POST -Path '/graphql' -Body @{ query = $treeQuery } -Anonymous
    Assert "§4.4 categoryTree responds 200" ($r.StatusCode -eq 200)
    $treeJson = $r.Body | ConvertFrom-Json
    Assert "§4.4 response has data.categoryTree" ($null -ne $treeJson.data.categoryTree)

    Write-Host "`n== §6. Guard rails ==" -ForegroundColor Cyan
    $r = Insert-Category -Name 'GuardRailNoParent' -ParentId 9999999999
    Assert "§6 non-existent parent rejected (4xx)" ($r.StatusCode -ge 400 -and $r.StatusCode -lt 500) -Detail "got $($r.StatusCode)"
    Assert "§6 non-existent parent error mentions 'Parent'" ($r.Body -match 'Parent|parent|not found')

    $dupName = "GuardRailDup-$([guid]::NewGuid().ToString('N').Substring(0,8))"
    $r1 = Insert-Category -Name $dupName -ParentId $rootJson.categoryId
    if ($r1.StatusCode -ge 200 -and $r1.StatusCode -lt 300) {
        $insertedIds += ($r1.Body | ConvertFrom-Json).categoryId
        $r2 = Insert-Category -Name $dupName -ParentId $rootJson.categoryId
        Assert "§6 sibling-name collision rejected" ($r2.StatusCode -ge 400 -and $r2.StatusCode -lt 500)
        Assert "§6 sibling-name error mentions existing" ($r2.Body -match 'exists|already')
    }

    $r = Update-Category -CategoryId $rootJson.categoryId -Name $rootName -ParentId $childJson.categoryId
    Assert "§6 cycle (rooted at descendant) rejected" ($r.StatusCode -ge 400 -and $r.StatusCode -lt 500)
    Assert "§6 cycle error mentions cycle" ($r.Body -match 'cycle|Cycle')

    $r = Delete-Category -CategoryId $rootJson.categoryId
    Assert "§6 delete-with-children rejected" ($r.StatusCode -ge 400 -and $r.StatusCode -lt 500)
    Assert "§6 delete error mentions subcategories" ($r.Body -match 'subcategories|children|remove them first')

    Write-Host "`n== §7. Cascade rename ==" -ForegroundColor Cyan
    $newRootName = "QSRenamed-$([guid]::NewGuid().ToString('N').Substring(0,8))"
    $r = Update-Category -CategoryId $rootJson.categoryId -Name $newRootName
    Assert "§7 rename root succeeds" ($r.StatusCode -ge 200 -and $r.StatusCode -lt 300)
    $renamedRoot = $r.Body | ConvertFrom-Json

    $r = Invoke-Lofn -Method POST -Path '/graphql' -Body @{ query = $treeQuery } -Anonymous
    $newTree = $r.Body | ConvertFrom-Json
    function Find-Slug {
        param($Tree, [long] $Id)
        foreach ($n in $Tree) {
            if ($n.categoryId -eq $Id) { return $n.slug }
            if ($n.children) {
                $deep = Find-Slug -Tree $n.children -Id $Id
                if ($deep) { return $deep }
            }
        }
        return $null
    }
    $newChildSlug = Find-Slug -Tree $newTree.data.categoryTree -Id $childJson.categoryId
    $newGrandSlug = Find-Slug -Tree $newTree.data.categoryTree -Id $grandJson.categoryId
    Assert "§7 child slug starts with renamed root '$($renamedRoot.slug)/'" ($null -ne $newChildSlug -and $newChildSlug.StartsWith("$($renamedRoot.slug)/"))
    Assert "§7 grandchild slug starts with renamed root '$($renamedRoot.slug)/'" ($null -ne $newGrandSlug -and $newGrandSlug.StartsWith("$($renamedRoot.slug)/"))
}
finally {
    Write-Host "`n== Cleanup ==" -ForegroundColor Cyan
    Cleanup
}

$total = $passed + $failed
Write-Host "`n== Summary == passed=$passed failed=$failed total=$total" -ForegroundColor (if ($failed -gt 0) { 'Red' } else { 'Green' })
exit ([int]($failed -gt 0))
