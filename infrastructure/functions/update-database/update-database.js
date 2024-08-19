// This function is called by our CI/CD
// pipeline after processing a SET/DEL
// request. It updates the Status and
// BuildDate values in our db table.
const { Client } = require('pg');

module.exports = async function (context, req) {
    try {
        // Retrieve inputs from the Logic App trigger
        const indexToUpdate = req.body.indexToUpdate;
        const operation = req.body.operation;
        const updateValue = req.body.updateValue;
        const tableToUpdate = 'container_images'; 

        // PostgreSQL connection (ensure connectionString is set as an environment variable)
        const client = new Client({
            connectionString: process.env.connectionString,
            ssl: { rejectUnauthorized: false } 
        });

        await client.connect();

        // SQL queries 
        const updateQuery = `
            UPDATE ${tableToUpdate}
            SET Status = $1
            WHERE RepositoryId = $2; 
        `;

        const deleteQuery = `
            DELETE FROM ${tableToUpdate}
            WHERE RepositoryId = $1; 
        `;

        const values = [updateValue, indexToUpdate];
        let result = null;

        // Execute db operation
        if (operation.toLowerCase() === 'update') {
            result = await client.query(updateQuery, values);
        } else if (operation.toLowerCase() === 'delete') {
            result = await client.query(deleteQuery, values);
        }

        if (result == null || result.rowCount === 0) {
            throw new Error(`No rows were edited in table ${tableToUpdate}. Check if the index exists.`);
        }

        context.log(`Successful ${operation} operation for row ${indexToUpdate} in table ${tableToUpdate}`);
        context.res = {
            body: { result: true }
        };

        await client.end();
    } catch (error) {
        context.log.error(error.message);
        context.res = {
            status: 500,
            body: { result: false, error: error.message }
        };
    }
};