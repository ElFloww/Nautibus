Get-ChildItem -Path .\ -Filter *.slnx -Recurse -File -Name| ForEach-Object {
    dotnet sln $_ list | Select-Object -Skip 2 | Foreach-Object {
        $lastIndex = $_.LastIndexOf('\')
        $tmp = $_.SubString(0, $lastIndex)
        $root = "$($PSScriptRoot)\$($tmp)";
        
        $binPath = "$($root)\bin"
        $objPath = "$($root)\obj"

        Write-Host "Cleaning $($root)"

        Remove-Item -path $binPath -recurse
        Remove-Item -path $objPath -recurse
    }

    dotnet restore $_
}