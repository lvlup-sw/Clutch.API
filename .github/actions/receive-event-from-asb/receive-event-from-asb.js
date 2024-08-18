// We use this action to fetch
// events from Azure Service Bus
// since the CLI bizarrely doesn't
// support event reception.

const { ServiceBusClient } = require('@azure/service-bus');
const core = require('@actions/core');

async function main() {
    try {
        // Get inputs from the workflow
        const connectionString = core.getInput('connectionString');
        const queueName = core.getInput('queueName');

        // Set as secret to mask in logs
        core.setSecret(connectionString);

        // Create a Service Bus client
        const sbClient = new ServiceBusClient(connectionString);
        const receiver = sbClient.createReceiver(queueName);

        // Receive a message (wait for max 20s)
        const messages = await receiver.receiveMessages(1, { maxWaitTimeInMs: 20000 });

        if (messages.length > 0) {
            const message = messages[0];

            core.info(JSON.stringify(message.applicationProperties));
            core.info(JSON.stringify(message.body));

            // Extract event name and data
            const eventName = message.applicationProperties.eventName;
            const eventData = message.body.eventData;

            // Set outputs for the workflow
            core.info(`Setting eventName output to ${JSON.stringify(eventName)}`)
            core.info(`Setting eventData output to ${JSON.stringify(eventData)}`)
            core.setOutput('eventName', JSON.stringify(eventName));
            core.setOutput('eventData', JSON.stringify(eventData));

            // Complete the message
            await receiver.completeMessage(message);
        } else {
            core.info('No messages received.');
        }

        // Close the receiver and client
        await receiver.close();
        await sbClient.close();
    } catch (error) {
        core.error(error.message);
        core.setFailed(`Exiting with error: ${error}`);
        process.exit(1);
    }
}

main();