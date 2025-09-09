import {v4} from 'uuid';

import {pool} from "../../util/database.js";

class BookService {
    async getAllBooks(limit = 10, offset = 0) {
        const client = await pool.connect();

        try {
            const query = `
            SELECT id, title, publish, author
            FROM books
            ORDER BY publish DESC
            LIMIT $1 OFFSET $2
            `;

            const result = await client.query(query, [limit, offset]);

            const countQuery = 'SELECT COUNT(*) FROM books';

            const countResult = await client.query(countQuery);

            return {
                books: result.rows,
                total: parseInt(countResult.rows[0].count),
                limit,
                offset
            };
        }
        catch (error) {
            console.log('An error occurred while fetching all books', error);
        }
        finally {
            client.release();
        }
    }
}

export default new BookService();