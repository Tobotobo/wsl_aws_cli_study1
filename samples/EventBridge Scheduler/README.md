# EventBridge Scheduler

floci - EventBridge Scheduler  
https://floci.io/floci/services/scheduler/  

## スケジュールグループの作成

```bash
aws scheduler create-schedule-group \
  --name my-group
```

## スケジュールグループ一覧の取得

```bash
aws scheduler list-schedule-groups
```

## スケジュールの作成

実行対象の Lambda の ARN を取得
```bash
LAMBDA_ARN=$(aws lambda get-function \
  --function-name hello-world-lambda \
  --query 'Configuration.FunctionArn' \
  --output text)

echo "$LAMBDA_ARN"
```

スケジュールを作成
```bash
aws scheduler create-schedule \
  --name my-schedule \
  --group-name my-group \
  --schedule-expression "rate(1 minutes)" \
  --flexible-time-window '{"Mode":"OFF"}' \
  --target '{
    "Arn": "arn:aws:lambda:ap-northeast-1:000000000000:function:hello-world-lambda",
    "RoleArn": "arn:aws:iam::000000000000:role/scheduler-role"
  }'
```

schedule-expression
* rate(value minutes|hours|days)
* cron(minutes hours day-of-month month day-of-week year)
* at(yyyy-mm-ddThh:mm:ss)

Schedule types in EventBridge Scheduler  
https://docs.aws.amazon.com/scheduler/latest/UserGuide/schedule-types.html  


## スケジュールの取得

```bash
aws scheduler get-schedule \
  --group-name my-group \
  --name my-schedule
```

## スケジュール一覧の取得

```bash
aws scheduler list-schedules \
  --query 'Schedules[*].[Name,State,GroupName]' \
  --output table
```

## スケジュール削除

```bash
aws scheduler delete-schedule \
  --group-name my-group \
  --name my-schedule
```

