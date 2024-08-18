// This function is called by our CI/CD
// pipeline after processing a SET/DEL
// request. It updates the Status and
// BuildDate values in our db table.

import { Client } from 'pg';
import { getInput, setOutput, setFailed } from '@actions/core';

// Define the specific table to update (replace with your actual table name)
const TABLE_TO_UPDATE = getInput('table_to_update', { required: true }); 

async function run() {
  try {
    // Get required inputs from the Action's YAML file
    const indexToUpdate = getInput('index_to_update', { required: true });
    const newValue = getInput('new_value', { required: true });

    // Get PostgreSQL connection details securely from GitHub secrets
    const connectionString = getInput('postgresql_connection_string', { required: true });

    // Create a PostgreSQL client
    const client = new Client({
      connectionString: connectionString,
      ssl: { rejectUnauthorized: false } 
    });

    // Connect to the database
    await client.connect();

    // Construct and execute the SQL UPDATE query
    const updateQuery = `
        UPDATE ${TABLE_TO_UPDATE}
        SET Status = $1
        WHERE RepositoryId = $2; 
    `;

    const values = [newValue, indexToUpdate];
    const result = await client.query(updateQuery, values);

    // Check if any rows were affected
    if (result.rowCount === 0) {
      throw new Error(`No rows were updated in table ${TABLE_TO_UPDATE}. Check if the index exists.`);
    }

    core.info(`Successfully updated Status to ${newValue} for row ${indexToUpdate} in table ${TABLE_TO_UPDATE}`);
    setOutput('result', true);

    // Release the connection
    await client.end();

  } catch (error) {
    core.error(error.message);
    setOutput('result', false);
  }
}

run();