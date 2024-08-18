// This function is called by our CI/CD
// pipeline after processing a SET/DEL
// request. It updates the Status and
// BuildDate values in our db table.

const { Client } = require('pg');
const { getInput, setOutput, setFailed } = require('@actions/core'); 

async function run() {
  try {
    // Get required inputs from the Action's YAML file
    const connectionString = getInput('postgresql_connection_string', { required: true });
    const tableToUpdate = getInput('table_to_update', { required: true });
    const indexToUpdate = getInput('index_to_update', { required: true });
    const operation = getInput('operation', { required: true });
    const updateValue = getInput('update_value', { required: true });
    
    // Create a PostgreSQL client
    const client = new Client({
      connectionString: connectionString,
      ssl: { rejectUnauthorized: false } 
    });

    // Connect to the database
    await client.connect();

    // Construct and execute the SQL queries
    const updateQuery = `
        UPDATE ${tableToUpdate}
        SET Status = $1
        WHERE RepositoryId = $2; 
    `;

    const deleteQuery = `
        DELETE FROM ${tableToUpdate}
        WHERE RepositoryId = $2; 
    `;

    const values = [updateValue, indexToUpdate];
    let result = null;

    // Execute operation
    if (operation.toLowerCase() === 'update') {
      result = await client.query(updateQuery, values);
    } else if (operation.toLowerCase() === 'delete') {
      result = await client.query(deleteQuery, values);
    }

    // Check if any rows were affected
    if (result == null || result.rowCount === 0) {
      throw new Error(`No rows were edited in table ${tableToUpdate}. Check if the index exists.`);
    }

    core.info(`Successfull ${operation} operation for row ${indexToUpdate} in table ${tableToUpdate}`);
    setOutput('result', true);

    // Release the connection
    await client.end();

  } catch (error) {
    core.error(error.message);
    setFailed(`Exiting with error: ${error}`);
    setOutput('result', false);
  }
}

run();