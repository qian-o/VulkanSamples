@echo off
setlocal enabledelayedexpansion

set SHADERS_DIR="Assets\Shaders"

for %%f in (%SHADERS_DIR%\*vs.hlsl) do (
	echo ±‡“Î %%f ...
	dxc -T vs_6_3 -E main -spirv -fvk-use-scalar-layout -fspv-target-env=vulkan1.3 -Fo %%f.spv %%f
	if !errorlevel! neq 0 (
		echo ±‡“Î %%f  ß∞‹
		exit /b !errorlevel!
	)
)

for %%f in (%SHADERS_DIR%\*ps.hlsl) do (
	echo ±‡“Î %%f ...
	dxc -T ps_6_3 -E main -spirv -fvk-use-scalar-layout -fspv-target-env=vulkan1.3 -Fo %%f.spv %%f
	if !errorlevel! neq 0 (
		echo ±‡“Î %%f  ß∞‹
		exit /b !errorlevel!
	)
)

echo À˘”–Œƒº˛±‡“ÎÕÍ≥…

endlocal