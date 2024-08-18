// This function is called by our CI/CD
// pipeline after processing a SET/DEL
// request. It updates the Status and
// BuildDate values in our db table.

const { Client } = require('pg');
const core = require('@actions/core'); 

async function main() {
  try {
      // Get required inputs from the Action's YAML file
      const db_user = core.getInput('db_user', { required: true });
      const db_password = core.getInput('db_password', { required: true });
      const db_host = core.getInput('db_host', { required: true });
      const db_port = core.getInput('db_port', { required: true });
      const db_name = core.getInput('db_name', { required: true });
      const tableToUpdate = core.getInput('table_to_update', { required: true });
      const indexToUpdate = core.getInput('index_to_update', { required: true });
      const operation = core.getInput('operation', { required: true });
      const updateValue = core.getInput('update_value', { required: true });

      // Set as secret to mask in logs
      core.setSecret(connectionString);

      // Create a PostgreSQL client
      const client = new Client({
        user: db_user,
        password: db_password,
        host: db_host,
        port: db_port,
        database: db_name,
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
        core.info(`Executing Update query: ${updateQuery}`)
        result = await client.query(updateQuery, values);
      } else if (operation.toLowerCase() === 'delete') {
        core.info(`Executing Delete query: ${deleteQuery}`)
        result = await client.query(deleteQuery, values);
      }

      // Check if any rows were affected
      if (result == null || result.rowCount === 0) {
        throw new Error(`No rows were edited in table ${tableToUpdate}. Check if the index exists.`);
      }

      core.info(`Successfull ${operation} operation for row ${indexToUpdate} in table ${tableToUpdate}`);
      core.setOutput('result', true);

      // Release the connection
      await client.end();
  } catch (error) {
      core.error(error.message);
      core.setFailed(`Exiting with error: ${error}`);
      core.setOutput('result', false);
      process.exit(1);
  }
}

main();