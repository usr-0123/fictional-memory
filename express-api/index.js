// import modules
import express from 'express';
import cors from 'cors';
import dotenv from 'dotenv';
import {testDbConnection} from "./src/util/database.js";
import router from "./src/controller/book/book.controller.js";

// create express application
dotenv.config();

const port = process.env.PORT || 3000;

const environment = process.env.NODE_ENV || 'development';

const allowedOrigin = process.env.CORS_ORIGIN || 'http://localhost:5173';

const version = process.env.APP_VERSION || '1.0.0';

const appName = process.env.APP_NAME || 'Express API';

const app = express();

// Middlewares

// Always put app.use() after all the routes, so it only triggers if nothing matches

// CORS configuration
app.use(cors({
    origin: allowedOrigin,
    credentials: true,
}));

// Body parsing
app.use(express.json({limit: '10mb'}));

app.use(express.urlencoded({extended: true, limit: '10mb'}));


// Health check endpoint
app.get('/health', (req,res) => {
    res.json({
        status: 'OK',
        message: 'Server is healthy',
        timestamp: new Date().toISOString(),
        environment: environment,
        uptime: process.uptime(),
        memory: process.memoryUsage()
    });
});

// create route and call them here
// API info endpoint
app.get('/api', (req,res) => {
    res.json({
        name: appName,
        version: version,
        environment: environment,
        endpoints: {
            health: '/health',
            api: '/api',
        }
    })
})


// TODO: Handle all the other routes here
app.use('/api/books', router)

// not implemented route
app.use((req,res) => {
    res.status(404).json({
        success: false,
        error: 'Route not found',
        path: req.originalUrl,
        message: `Cannot ${req.method} ${req.originalUrl}`,
        timestamp: new Date().toISOString()
    });
});


// Global error handler
app.use((err, req, res, next) => {
    console.error('Something went wrong', {
        message: err.message,
        stack: err.stack,
        url: req.originalUrl,
        method: req.method,
        timestamp: new Date().toISOString()
    });

    const message = environment === 'production'
    ? 'Internal server error'
        : err.message;

    res.status(err.statusCode || 500).json({
        success: false,
        error: message,
        ...(environment === 'development' && {stack: err.stack})
    });
});

// Graceful shutdown handlers
const gracefulShutdown = (signal) => {
    console.log(`\n${signal} received. STarting graceful shutdown...`);

    server.close((err) => {
        if (err) {
            console.log('Error during server close:', err);
            process.exit(1);
        }

        console.log('Server closed successfully');
        process.exit(0);
    });

    setTimeout(() => {
        console.error('Force closing server after 30 seconds');
        process.exit(1);
    }, 3000);
};

// handle unhandled promise rejections
process.on('unhandledRejection', (reason, promise) => {
    console.error('Unhandled promise rejection:', {
        reason: reason,
        promise: promise,
        timestamp: new Date().toISOString()
    });

    server.close(() => {
        process.exit(1);
    });
});

// handle uncaught exceptions
process.on('uncaughtException', (err) => {
    console.error('Uncaught Exception:', {
        message: error.message,
        stack: err.stack,
        timestamp: new Date().toISOString()
    });

    process.exit(1);
});

// Handle shutdown on termination signals
process.on('SIGTERM', () => gracefulShutdown('SIGTERM'));
process.on('SIGINT', () => gracefulShutdown('SIGINT'));


// start the server
const server = app.listen(port, async () => {
    console.log(`Server is running on port ${port}`);
    console.log(`Environment: ${environment}`);
    console.log(`Visit http://localhost:${port}/health to check server status`);

    // Test database connection
    const dbConn = await testDbConnection();

    if (!dbConn) {
        console.log('Server started but database connection failed');
    }

});

// Handle server start up errors
server.on('error', (err) => {
    if (err.code === 'EADDRINUSE') {
        console.error(`Port number ${port} is already in use`);
        process.exit(1);
    } else {
        console.error('Server startup error', err);
        process.exit(1);
    }
});


export default app;
