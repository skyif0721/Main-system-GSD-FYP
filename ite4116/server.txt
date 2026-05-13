const { MongoClient } = require('mongodb');
const express = require('express');
const cors = require('cors');
const app = express();

// Allow Unity to connect
app.use(cors());
app.use(express.json());

// YOUR MONGODB CONNECTION STRING
const uri = "mongodb+srv://yeungkachun2006_db_user:fbfN1VM1sCtR31sT@ite4116.bhwn8nq.mongodb.net/GameDB?retryWrites=true&w=majority";
const client = new MongoClient(uri);

async function startServer() {
  try {
    // Connect to MongoDB
    await client.connect();
    console.log("Connected to MongoDB Atlas");

    const db = client.db("GameDB");
    const playersCollection = db.collection("Players");

    // ======================================
    // 1. SAVE Player (Unity sends data here)
    // ======================================
    app.post("/api/save", async (req, res) => {
      const playerData = req.body;
      await playersCollection.updateOne(
        { id: playerData.id },
        { $set: playerData },
        { upsert: true }
      );
      res.json({ success: true });
    });

    // ======================================
    // 2. LOAD Player (Unity gets data here)
    // ======================================
    app.get("/api/load/:id", async (req, res) => {
        const playerID = req.params.id;
        const player = await playersCollection.findOne({ id: playerID });
        res.json(player);
    });

    const PORT = process.env.PORT || 3000;
    app.listen(PORT, () => {
        console.log("Server running on port " + PORT);
    });


  } catch (err) {
    console.error("Error:", err);
  }
}

startServer();