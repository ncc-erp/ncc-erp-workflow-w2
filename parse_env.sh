#!/bin/bash

cp appsettings_exp.json  appsettings.json 

export KOMU_API_URL=$(echo "$KOMU_API_URL" | sed 's/\//\\\//g' )
export API_URL=$(echo "$API_URL" | sed 's/\//\\\//g' )
export URL_WEB=$(echo "$URL_WEB" | sed 's/\//\\\//g' )
export MEZON_REDIRECT_URI=$(echo "https://w2-dev2.nccsoft.vn/callback" | sed 's/\//\\\//g' )

sed "s/KOMU_API_URL/$KOMU_API_URL/g" -i appsettings.json
sed "s/KOMU_X_SK/$KOMU_X_SK/g" -i appsettings.json
sed "s/API_URL/$API_URL/g" -i appsettings.json
sed "s/DB_HOST/$DB_HOST/g" -i appsettings.json
sed "s/DB_NAME/$DB_NAME/g" -i appsettings.json
sed "s/DB_PASS/$DB_PASS/g" -i appsettings.json
sed "s/DB_PORT/$DB_PORT/g" -i appsettings.json
sed "s/DB_USER/$DB_USER/g" -i appsettings.json
sed "s/PASSPHRASE/$PASSPHRASE/g" -i appsettings.json
sed "s/SES_HOST/$SES_HOST/g" -i appsettings.json
sed "s/SES_PORT/$SES_PORT/g" -i appsettings.json
sed "s/SES_SENDER/$SES_SENDER/g" -i appsettings.json
sed "s/SES_USER/$SES_USER/g" -i appsettings.json
sed "s/SES_PASS/$SES_PASS/g" -i appsettings.json
sed "s/SES_REQ_CRE/$SES_REQ_CRE/g" -i appsettings.json
sed "s/KOMU_API_URL/$KOMU_API_URL/g" -i appsettings.json
sed "s/KOMU_X_SK/$KOMU_X_SK/g" -i appsettings.json
sed "s/GG_CLIENT/$GG_CLIENT/g" -i appsettings.json
sed "s/GG_SECRET/$GG_SECRET/g" -i appsettings.json
sed "s/TALENT_DF/$TALENT_DF/g" -i appsettings.json
sed "s/URL_WEB/$URL_WEB/g" -i appsettings.json
sed "s/WFH_DEFINITIONS_ID/$WFH_DEFINITIONS_ID/g" -i appsettings.json
sed "s/API_SECRET_KEY_HEADER_NAME/$API_SECRET_KEY_HEADER_NAME/g" -i appsettings.json
sed "s/API_X_SECRET_KEY/$API_X_SECRET_KEY/g" -i appsettings.json
sed "s/JWT_SECRET/$JWT_SECRET/g" -i appsettings.json
sed "s/MEZON_CLIENT/$MEZON_CLIENT/g" -i appsettings.json
sed "s/MEZON_SECRET/$MEZON_SECRET/g" -i appsettings.json
sed "s/MEZON_REDIRECT_URI/$MEZON_REDIRECT_URI/g" -i appsettings.json
sed "s/HRM_API_URL/$HRM_API_URL/g" -i appsettings.json
sed "s/HRM_API_X_SECRET_HEADER/$HRM_API_X_SECRET_HEADER/g" -i appsettings.json
