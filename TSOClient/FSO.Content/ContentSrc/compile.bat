echo "================================================================"
echo "=== Make sure you have VC++ Redistributables 2013 installed! ==="
echo "================================================================"

pause

echo "=== Installing MGCB ==="
dotnet tool restore

echo "=== Removing compiled assets ==="
rmdir /S /Q ..\Content\DX\*
rmdir /S /Q ..\Content\OGL\*

echo "=== Compiling DirectX Shaders ==="
dotnet mgcb /b TSOClientContentDX.mgcb /outputDir:..\Content\DX
rmdir /S /Q obj\

echo "=== Compiling OpenGL Shaders ==="
dotnet mgcb /b TSOClientContent.mgcb /outputDir:..\Content\OGL
rmdir /S /Q obj\
