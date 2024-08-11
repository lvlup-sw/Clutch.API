// This function is called by our CI/CD
// pipeline after processing a SET/DEL
// request. It updates the Status and
// BuildDate values in our db table.

const { Client } = require('pg');

module.exports = async function (context, mySbMsg) {
    context.log('JavaScript ServiceBus queue trigger function processed message', mySbMsg);

    try {
        // Extract relevant data from the message (adjust based on your message structure)
        const messageBody = JSON.parse(mySbMsg.body);
        const { tableName, columnToUpdate, newValue, whereCondition } = messageBody;

        // Create a PostgreSQL client with connection pooling
        const client = new Client({
            connectionString: process.env.POSTGRESQL_CONNECTION_STRING, // Store in App Settings
            ssl: { rejectUnauthorized: false } // Adjust based on your PostgreSQL server configuration
        });

        // Connect to the database
        await client.connect();

        // Construct and execute the SQL UPDATE query
        const updateQuery = `
            UPDATE ${tableName}
            SET ${columnToUpdate} = $1
            WHERE ${whereCondition};
        `;

        const values = [newValue];
        await client.query(updateQuery, values);

        context.log('Database updated successfully!');

        // Release the connection back to the pool
        await client.end();

        // Complete the message
        context.done();

    } catch (error) {
        context.log.error('Error updating database:', error);

        // Abandon the message for reprocessing (adjust based on your Azure Function setup)
        // You might need to throw an error or use a specific context method to abandon
    }
};
