$framework = '4.0'
Include .\version.ps1

properties {
    $config= if($config -eq $null) {'Debug' } else {$config}
    $base_dir = resolve-path .\..
    $source_dir = "$base_dir\src"
    $tools_dir = "$base_dir\tools"
    $out_dir = "$base_dir\out\$config"
    $infura_dir = "$source_dir\infura"
}

task clean {
    rd "$infura_dir\infura\bin" -recurse -force  -ErrorAction SilentlyContinue | out-null
    mkdir "$infura_dir\infura\bin"  -ErrorAction SilentlyContinue  | out-null

    rd "$infura_dir\infura.resclient\bin" -recurse -force  -ErrorAction SilentlyContinue | out-null
    mkdir "$infura_dir\infura.resclient\bin"  -ErrorAction SilentlyContinue  | out-null

    rd "$infura_dir\infura.tests\bin" -recurse -force  -ErrorAction SilentlyContinue | out-null
    mkdir "$infura_dir\infura.tests\bin"  -ErrorAction SilentlyContinue  | out-null   
}

task version -depends clean {
     $commitHashAndTimestamp = Get-GitCommitHashAndTimestamp
     $commitHash = Get-GitCommitHash
     $timestamp = Get-GitTimestamp
     $branchName = Get-GitBranchOrTag
	 
	 $assemblyInfos = Get-ChildItem -Path $base_dir -Recurse -Filter AssemblyInfo.cs

	 $assemblyInfo = gc "$base_dir\AssemblyInfo.pson" | Out-String | iex
	 $version = $assemblyInfo.Version
	 #$productName = $assemblyInfo.ProductName
	 $companyName = $assemblyInfo.CompanyName
	 $copyright = $assemblyInfo.Copyright

	 try {
        foreach ($assemblyInfo in $assemblyInfos) {
            $path = resolve-Path $assemblyInfo.FullName -Relative
            #Write-Host "Patching $path with product information."
            Patch-AssemblyInfo $path $Version $Version $branchName $commitHashAndTimestamp $companyName $copyright
        }         
    } catch {
        foreach ($assemblyInfo in $assemblyInfos) {
            $path = resolve-Path $assemblyInfo.FullName -Relative
            Write-Host "Reverting $path to original state."
            & { git checkout --quiet $path }
        }
    }	
}

task compile -depends version {
	try{
		exec { msbuild $infura_dir\infura.sln /t:Clean /t:Build /p:Configuration=$config /v:q /nologo }
	} finally{
		$assemblyInfos = Get-ChildItem -Path $base_dir -Recurse -Filter AssemblyInfo.cs
		foreach ($assemblyInfo in $assemblyInfos) {
            $path = Resolve-Path $assemblyInfo.FullName -Relative
            Write-Verbose "Reverting $path to original state."
            & { git checkout --quiet $path }
        }
	}
}

task nuget -depends nuget-infura-resclient

task nuget-infura-resclient -depends build-infura-resclient, publish-infura-resclient

task build-infura-resclient -depends compile {
	$commitHashAndTimestamp = Get-GitCommitHashAndTimestamp
    $commitHash = Get-GitCommitHash
    $timestamp = Get-GitTimestamp
    $branchName = Get-GitBranchOrTag
	
	$assemblyInfos = Get-ChildItem -Path $base_dir -Recurse -Filter AssemblyInfo.cs

	$assemblyInfo = gc "$base_dir\AssemblyInfo.pson" | Out-String | iex
	$version = $assemblyInfo.Version
	#$productName = $assemblyInfo.ProductName
	$companyName = $assemblyInfo.CompanyName
	$copyright = $assemblyInfo.Copyright

	try {
       foreach ($assemblyInfo in $assemblyInfos) {
           $path = resolve-Path $assemblyInfo.FullName -Relative
           #Write-Host "Patching $path with product information."
           Patch-AssemblyInfo $path $Version $Version $branchName $commitHashAndTimestamp $companyName $copyright
       }         
    } catch {
        foreach ($assemblyInfo in $assemblyInfos) {
            $path = resolve-Path $assemblyInfo.FullName -Relative
            Write-Host "Reverting $path to original state."
            & { git checkout --quiet $path }
        }
    }
	
	try{
		Push-Location "$infura_dir\Infura.ResClient"
		exec { & "$infura_dir\.nuget\NuGet.exe" "spec"}
		exec { & "$infura_dir\.nuget\nuget.exe" pack Infura.ResClient.csproj -IncludeReferencedProjects}
	} finally{
		Pop-Location
		$assemblyInfos = Get-ChildItem -Path $base_dir -Recurse -Filter AssemblyInfo.cs
		foreach ($assemblyInfo in $assemblyInfos) {
            $path = resolve-Path $assemblyInfo.FullName -Relative
            #Write-Verbose "Reverting $path to original state."
            & { git checkout --quiet $path }
        }
	}	
}

task publish-infura-resclient -depends build-infura-resclient {
	$pkgPath = Get-ChildItem -Path "$infura_dir\infura.ResClient" -Filter "*.nupkg" | select-object -first 1
	exec { & "$infura_dir\.nuget\nuget.exe" push "$infura_dir\infura.ResClient\$pkgPath" }
	ri "$infura_dir\infura.ResClient\$pkgPath"
}
