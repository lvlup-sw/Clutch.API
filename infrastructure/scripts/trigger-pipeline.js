// This code is run as a serverless function
// to trigger the CI/CD event workflow when
// there is a new message in the event queue.
// The workflow is triggered via webhook.

module.exports = async function (context, mySbMsg) {
    context.log('JavaScript ServiceBus queue trigger function processed message', mySbMsg);

    try {
        // Extract event name and data from the message (adjust parsing as needed)
        const messageBody = JSON.parse(mySbMsg.body);
        const eventName = messageBody.eventName;
        const eventData = messageBody.data;

        // Make a POST request to the GitHub Webhook
        const response = await fetch(
            'https://api.github.com/repos/{owner}/{repo}/dispatches', // Replace with your actual repository details
            {
                method: 'POST',
                headers: {
                    'Accept': 'application/vnd.github.v3+json',
                    'Authorization': `token ${process.env.GITHUB_WEBHOOK_SECRET}`, // Use your GitHub secret token
                },
                body: JSON.stringify({
                    'event_type': eventName,
                    'client_payload': eventData,
                })
            }
        );

        if (response.status === 204) {
            context.log('Successfully triggered GitHub workflow.');
            context.done(); // Complete the message after successful processing
        } else {
            context.log.error(`Failed to trigger GitHub workflow. Status code: ${response.status}`);
            // Abandon the message if the webhook call fails (adjust based on your Azure Function setup)
            // You might need to throw an error or use a specific context method to abandon
        }

    } catch (error) {
        context.log.error(`An error occurred: ${error.message}`);
        // Abandon the message in case of any other errors (adjust based on your Azure Function setup)
        // You might need to throw an error or use a specific context method to abandon
    }
};