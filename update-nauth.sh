#!/bin/bash
cd ../NAuth/Backend/NAuth
pwd
dotnet build -c Release NAuth.sln
cd ./NAuth.ACL/bin/Release/net8.0
pwd
cp NAuth.ACL.dll ../../../../../../../MonexUp/Backend/MonexUp/Lib
cp NAuth.DTO.dll ../../../../../../../MonexUp/Backend/MonexUp/Lib
cd ../../../../../../../NAuth/Frontend/nauth-core
pwd
npm install --legacy-peer-deps
npm run build
rm -Rf ../../../MonexUp/Frontend/monexup-app/src/lib/nauth-core
cp -Rf ./dist ../../../MonexUp/Frontend/monexup-app/src/lib/nauth-core
