// Since the Azure CLI bizarrely does not
// support direct message reception, I
// created a custom github action using
// Azure Service Bus client library :)

const { ServiceBusClient } = require("@azure/service-bus");
const core = require('@actions/core');

async function main() {
    try {
        // Get inputs from the workflow
        const connectionString = core.getInput("connectionString");
        const queueName = core.getInput("queueName");

        // Set as secret to mask in logs
        core.setSecret(connectionString);

        // Create a Service Bus client
        const sbClient = new ServiceBusClient(connectionString);
        const receiver = sbClient.createReceiver(queueName);

        // Receive a message
        const messages = await receiver.receiveMessages(1);

        if (messages.length > 0) {
            const message = messages[0];

            // Parse the message body (assuming it's JSON)
            const messageBody = JSON.parse(message.body);

            // Extract event name and data
            const eventName = messageBody.eventName;
            const eventData = messageBody.data;

            // Set outputs for the workflow
            core.setOutput("eventName", eventName);
            core.setOutput("eventData", JSON.stringify(eventData));

            // Complete the message
            await receiver.completeMessage(message);
        } else {
            core.info("No messages received.");
        }

        // Close the receiver and client
        await receiver.close();
        await sbClient.close();
    } catch (error) {
        core.setFailed(error.message);
    }
}

main();