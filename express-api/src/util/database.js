// import modules
import pg from 'pg';
import dotenv from 'dotenv';

// initialize dotenv
dotenv.config();

const {Pool} = pg;

// Database configs
const dbConfig = {
    user: process.env.DB_USER || 'postgres',
    host: process.env.DB_HOST || 'localhost',
    database: process.env.DB_NAME || 'express_api',
    password: process.env.DB_PASSWORD || 'password',
    port: process.env.DB_PORT || 5432,

    max: 20, // maximum number of connections
    idleTimeoutMillis: 30000, // Close idle connections after 30 seconds
    connectionTimeoutMillis: 2000, // return error after 2 seconds if connection could not be established
    maxUses: 7500, // Close connection after 7500 uses

    // ssl configuration
    ssl: process.env.ENVIRONMENT === 'production' ? {rejectUnauthorized: false} : false
};

// Alternatively use of connection string
/*
const connectionString = process.env.DB_URL;

const pool = new Pool({
    connectionString: connectionString,
    ssl: process.env.ENVIRONMENT === 'production' ? {rejectUnauthorized: false} : false,
});
*/

const pool = new Pool(dbConfig);


const testDbConnection = async () => {
    try {
        const client = await pool.connect();
        console.log('PostgreSQL connected successfully');

        // Test query
        const result = await client.query('SELECT NOW()');
        console.log(`Database time:`, result.rows[0].now);

        client.release();

        return true;

    } catch (error) {
        console.error('PostgreSQL connection failed:', error.message);
        return false;
    }
};

const closePool = async () => {
    try {
        await pool.end();
        console.log('Database pool closed')
    } catch (error) {
        console.error('Error closing database pool:', error);

    }
}

// handle pool errors
pool.on('error', (err,client) => {
    console.error('Database pool error:', err);
});

export {pool, testDbConnection, closePool};