TOP=../../../../..

include ../wasm.mk

ifneq ($(AOT),)
override MSBUILD_ARGS+=/p:RunAOTCompilation=true
endif

PROJECT_NAME=Wasm.Browser.Shutdown.Sample.csproj

run: run-browser
