const { Client } = require('pg');

// Define the specific table to update (replace with your actual table name)
const TABLE_TO_UPDATE = 'your_table_name';

module.exports = async function (context, mySbMsg) {
    context.log('JavaScript ServiceBus queue trigger function processed message', mySbMsg);

    try {
        // Extract index and value from the message body
        const messageBody = JSON.parse(mySbMsg.body);
        const indexToUpdate = Object.keys(messageBody)[0]; // Assuming the first key is the index
        const newValue = messageBody[indexToUpdate];

        // Create a PostgreSQL client
        const client = new Client({
            connectionString: process.env.POSTGRESQL_CONNECTION_STRING,
            ssl: { rejectUnauthorized: false } 
        });

        // Connect to the database
        await client.connect();

        // Construct and execute the SQL UPDATE query
        const updateQuery = `
            UPDATE ${TABLE_TO_UPDATE}
            SET value = $1
            WHERE id = $2; 
        `;

        const values = [newValue, indexToUpdate];
        await client.query(updateQuery, values);

        context.log('Database updated successfully!');

        // Release the connection
        await client.end();

        // Complete the message
        context.done();

    } catch (error) {
        context.log.error('Error updating database:', error);
        // Handle error appropriately (e.g., abandon the message)
    }
};