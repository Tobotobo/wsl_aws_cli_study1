export const handler = async (event, context) => {
  console.info("Lambda invoked");

  console.info("Event:", JSON.stringify(event, null, 2));

  console.info("Context:", JSON.stringify({
    requestId: context.awsRequestId,
    functionName: context.functionName,
    functionVersion: context.functionVersion,
    invokedFunctionArn: context.invokedFunctionArn,
    memoryLimitInMB: context.memoryLimitInMB,
    remainingTimeInMillis: context.getRemainingTimeInMillis()
  }, null, 2));

  const responseBody = {
    message: "Hello, World!"
  };

  const response = {
    statusCode: 200,
    headers: {
      "content-type": "application/json"
    },
    body: JSON.stringify(responseBody)
  };

  console.info("Response:", JSON.stringify(response, null, 2));

  return response;
};