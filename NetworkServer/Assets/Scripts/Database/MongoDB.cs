using System.Collections;
using System.Collections.Generic;
using MongoDB.Driver;
using UnityEngine;

public class Mongo
{
    private const string MONGO_URI = "";
    private const string DATABASE_NAME = "";

    private MongoClient _mongoClient;
    private MongoServer _mongoServer;
    private MongoDatabase _mongoDatabase;

    public void Init()
    {
        _mongoClient = new MongoClient(MONGO_URI);
        _mongoServer = _mongoClient.GetServer();
        _mongoDatabase = _mongoServer.GetDatabase(DATABASE_NAME);
        
        //This is where we would initialize collections
        Debug.Log("Database hase been initialized");
    }
    
    public void Shutdown()
    {
        _mongoClient = null;
        _mongoServer.Shutdown();
        _mongoDatabase = null;
    }
}
