#!/usr/bin/env bash 
dotnet publish -p:PublishSingleFile=true -r linux-x64 -c Release --self-contained -p:EnableCompressionInSingleFile=true 
