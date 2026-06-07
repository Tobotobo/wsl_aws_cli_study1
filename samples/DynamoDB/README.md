# DynamoDB

floci - DynamoDB  
https://floci.io/floci/services/dynamodb/  

【初心者向け】Amazon DynamoDB 入門！完全ガイド  
https://zenn.dev/issy/articles/zenn-dynamodb-overview  


## テーブル作成

```bash
aws dynamodb create-table \
  --table-name Users \
  --attribute-definitions \
    AttributeName=userId,AttributeType=S \
  --key-schema \
    AttributeName=userId,KeyType=HASH \
  --billing-mode PAY_PER_REQUEST
```

## アイテム追加

```bash
aws dynamodb put-item \
  --table-name Users \
  --item '{"userId":{"S":"tokyo.taro"},"name":{"S":"東京 太郎"},"age":{"N":"30"}}'
```

```bash
aws dynamodb put-item \
  --table-name Users \
  --item '{"userId":{"S":"tokyo.hanako"},"name":{"S":"東京 花子"},"age":{"N":"20"}}'
```

## アイテム取得

```bash
aws dynamodb get-item \
  --table-name Users \
  --key '{"userId":{"S":"tokyo.taro"}}'
```

## 主キー検索

```bash
aws dynamodb query \
  --table-name Users \
  --key-condition-expression "userId = :userId" \
  --expression-attribute-values '{":userId":{"S":"tokyo.taro"}}'
```

## 全件検索

```bash
aws dynamodb scan \
  --table-name Users
```

## 条件付き検索

```bash
aws dynamodb scan \
  --table-name Users \
  --filter-expression "age > :min" \
  --expression-attribute-values '{":min":{"N":"25"}}'
```

## アイテム削除

```bash
aws dynamodb delete-item \
  --table-name Users \
  --key '{"userId":{"S":"tokyo.taro"}}'
```

## テーブル削除

```bash
aws dynamodb delete-table \
  --table-name Users
```