@ ECHO OFF
FOR %%I IN (16,32,64,128,256) DO (
echo.
echo Compiling NNEDI3 %%I Neurons...
echo.
start fxc.exe /nologo /D nns=%%I /D VECTOR_DOT=1 /D UNROLLED=1 /T ps_5_0 /O3 /Fo nnedi3_%%I_C.cso nnedi3.hlsl
start fxc.exe /nologo /D nns=%%I /D LOOP_INNER=1 /T ps_5_0 /O3 /Fo nnedi3_%%I_D.cso nnedi3.hlsl
start fxc.exe /nologo /D nns=%%I /D LOOP_INNER=1 /D VECTOR_DOT=1 /T ps_5_0 /O3 /Fo nnedi3_%%I_E.cso nnedi3.hlsl
start fxc.exe /nologo /D nns=%%I /T ps_5_0 /O3 /Fo nnedi3_%%I_A.cso nnedi3.hlsl
start fxc.exe /nologo /D nns=%%I /D VECTOR_DOT=1 /T ps_5_0 /O3 /Fo nnedi3_%%I_B.cso nnedi3.hlsl
)
echo.
echo.