#!/bin/bash
echo "=== Removing compiled assets ==="
rm ../Content/DX/Effects/*.xnb ../Content/DX/Fonts/*.xnb
rm ../Content/OGL/Effects/*.xnb ../Content/OGL/Fonts/*.xnb


echo "=== Compiling DirectX Shaders ==="
mgcb -b TSOClientContentDX.mgcb
mv obj/Effects/*.xnb ../Content/DX/Effects
mv obj/Fonts/*.xnb ../Content/DX/Fonts
rm -rf obj/

echo "=== Compiling OpenGL Shaders ==="
mgcb -b TSOClientContent.mgcb
mv obj/Effects/*.xnb ../Content/OGL/Effects
mv obj/Fonts/*.xnb ../Content/OGL/Fonts
rm -rf obj/
