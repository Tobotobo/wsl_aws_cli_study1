// .NET ファイル ベースのアプリ
// https://learn.microsoft.com/ja-jp/dotnet/core/sdk/file-based-apps
// 実行: dotnet run ./samples/DynamoDB/HelloDynamoDB.cs

#:package AWSSDK.DynamoDBv2@*

using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

const string TableName = "Users";

using var dynamoDbClient = new AmazonDynamoDBClient();

// テーブル作成
await CreateTableAsync();
await WaitUntilTableActiveAsync();

// アイテム作成
await PutItemAsync("tokyo.taro", "東京 太郎", 30);
await PutItemAsync("tokyo.hanako", "東京 花子", 20);

// アイテム取得
await GetItemAsync("tokyo.taro");

// 主キー検索
await QueryByUserIdAsync("tokyo.taro");

// 全件検索
await ScanAllAsync();

// 条件付き検索
await ScanByAgeGreaterThanAsync(25);

// アイテム削除
await DeleteItemAsync("tokyo.taro");

// テーブル削除
await DeleteTableAsync();

// ------------------------------------------------------------------

async Task CreateTableAsync()
{
    try
    {
        var request = new CreateTableRequest
        {
            TableName = TableName,
            AttributeDefinitions =
            [
                new AttributeDefinition
                {
                    AttributeName = "userId",
                    AttributeType = ScalarAttributeType.S
                }
            ],
            KeySchema =
            [
                new KeySchemaElement
                {
                    AttributeName = "userId",
                    KeyType = KeyType.HASH
                }
            ],
            BillingMode = BillingMode.PAY_PER_REQUEST
        };

        await dynamoDbClient.CreateTableAsync(request);

        Console.WriteLine("テーブル作成:");
        Console.WriteLine("Users テーブルを作成しました。");
        Console.WriteLine();
    }
    catch (ResourceInUseException)
    {
        Console.WriteLine("テーブル作成:");
        Console.WriteLine("Users テーブルは既に存在します。");
        Console.WriteLine();
    }
}

async Task WaitUntilTableActiveAsync()
{
    while (true)
    {
        var response = await dynamoDbClient.DescribeTableAsync(TableName);

        if (response.Table.TableStatus == TableStatus.ACTIVE)
        {
            Console.WriteLine("Users テーブルが利用可能になりました。");
            Console.WriteLine();
            return;
        }

        await Task.Delay(1000);
    }
}

async Task PutItemAsync(string userId, string name, int age)
{
    var request = new PutItemRequest
    {
        TableName = TableName,
        Item = new Dictionary<string, AttributeValue>
        {
            ["userId"] = new AttributeValue { S = userId },
            ["name"] = new AttributeValue { S = name },
            ["age"] = new AttributeValue { N = age.ToString() }
        }
    };

    await dynamoDbClient.PutItemAsync(request);

    Console.WriteLine("アイテム追加:");
    Console.WriteLine($"userId={userId}, name={name}, age={age} を追加しました。");
    Console.WriteLine();
}

async Task GetItemAsync(string userId)
{
    var request = new GetItemRequest
    {
        TableName = TableName,
        Key = new Dictionary<string, AttributeValue>
        {
            ["userId"] = new AttributeValue { S = userId }
        }
    };

    var response = await dynamoDbClient.GetItemAsync(request);

    Console.WriteLine("アイテム取得:");
    Console.WriteLine($"userId={userId}");

    if (response.Item.Count == 0)
    {
        Console.WriteLine("該当するアイテムはありません。");
    }
    else
    {
        PrintItem(response.Item);
    }

    Console.WriteLine();
}

async Task QueryByUserIdAsync(string userId)
{
    var request = new QueryRequest
    {
        TableName = TableName,
        KeyConditionExpression = "userId = :userId",
        ExpressionAttributeValues = new Dictionary<string, AttributeValue>
        {
            [":userId"] = new AttributeValue { S = userId }
        }
    };

    var response = await dynamoDbClient.QueryAsync(request);

    Console.WriteLine("主キー検索:");
    Console.WriteLine($"userId={userId}");

    PrintItems(response.Items);
}

async Task ScanAllAsync()
{
    var request = new ScanRequest
    {
        TableName = TableName
    };

    var response = await dynamoDbClient.ScanAsync(request);

    Console.WriteLine("全件検索:");
    PrintItems(response.Items);
}

async Task ScanByAgeGreaterThanAsync(int minAge)
{
    var request = new ScanRequest
    {
        TableName = TableName,
        FilterExpression = "age > :min",
        ExpressionAttributeValues = new Dictionary<string, AttributeValue>
        {
            [":min"] = new AttributeValue { N = minAge.ToString() }
        }
    };

    var response = await dynamoDbClient.ScanAsync(request);

    Console.WriteLine("条件付き検索:");
    Console.WriteLine($"age > {minAge}");

    PrintItems(response.Items);
}

async Task DeleteItemAsync(string userId)
{
    var request = new DeleteItemRequest
    {
        TableName = TableName,
        Key = new Dictionary<string, AttributeValue>
        {
            ["userId"] = new AttributeValue { S = userId }
        }
    };

    await dynamoDbClient.DeleteItemAsync(request);

    Console.WriteLine("アイテム削除:");
    Console.WriteLine($"userId={userId} を削除しました。");
    Console.WriteLine();
}

async Task DeleteTableAsync()
{
    try
    {
        await dynamoDbClient.DeleteTableAsync(new DeleteTableRequest
        {
            TableName = TableName
        });

        Console.WriteLine("テーブル削除:");
        Console.WriteLine("Users テーブルを削除しました。");
        Console.WriteLine();
    }
    catch (ResourceNotFoundException)
    {
        Console.WriteLine("テーブル削除:");
        Console.WriteLine("Users テーブルは存在しません。");
        Console.WriteLine();
    }
}

void PrintItems(List<Dictionary<string, AttributeValue>> items)
{
    if (items.Count == 0)
    {
        Console.WriteLine("該当するアイテムはありません。");
        Console.WriteLine();
        return;
    }

    foreach (var item in items)
    {
        PrintItem(item);
    }

    Console.WriteLine();
}

void PrintItem(Dictionary<string, AttributeValue> item)
{
    var userId = item.TryGetValue("userId", out var userIdValue)
        ? userIdValue.S
        : "";

    var name = item.TryGetValue("name", out var nameValue)
        ? nameValue.S
        : "";

    var age = item.TryGetValue("age", out var ageValue)
        ? ageValue.N
        : "";

    Console.WriteLine($"userId: {userId}");
    Console.WriteLine($"name: {name}");
    Console.WriteLine($"age: {age}");
    Console.WriteLine();
}