// We use this action to fetch
// events from Azure Service Bus
// since the CLI bizarrely doesn't
// support event reception.

import { ServiceBusClient } from '@azure/service-bus';
import { getInput, setSecret, setOutput, info, setFailed } from '@actions/core';

async function main() {
    try {
        // Get inputs from the workflow
        const connectionString = getInput('connectionString');
        const queueName = getInput('queueName');

        // Set as secret to mask in logs
        setSecret(connectionString);

        // Create a Service Bus client
        const sbClient = new ServiceBusClient(connectionString);
        const receiver = sbClient.createReceiver(queueName);

        // Receive a message (wait for max 5s)
        const messages = await receiver.receiveMessages(1, { maxWaitTimeInMs: 5000 });

        if (messages.length > 0) {
            const message = messages[0];

            // Parse the message body
            const messageBody = JSON.parse(message.body);

            // Extract event name and data
            const eventName = messageBody.eventName;
            const eventData = messageBody.eventData;

            // Set outputs for the workflow
            setOutput('eventName', eventName);
            setOutput('eventData', JSON.stringify(eventData));

            // Complete the message
            await receiver.completeMessage(message);
        } else {
            info('No messages received.');
        }

        // Close the receiver and client
        await receiver.close();
        await sbClient.close();
    } catch (error) {
        setFailed(error.message);
    }
}

main();