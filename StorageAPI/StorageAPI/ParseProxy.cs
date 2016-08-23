﻿using ArtificialNeuralNetwork;
using ArtificialNeuralNetwork.Factories;
using ArtificialNeuralNetwork.Genes;
using NeuralNetwork.GeneticAlgorithm;
using Newtonsoft.Json;
using Parse;
using System.Collections.Generic;

namespace StorageAPI
{
    public class ParseProxy : IParseProxy 
    {
        private readonly string _networkVersion;

        public ParseProxy(string networkVersion, string appId, string dotNetKey)
        {
            _networkVersion = networkVersion;
            ParseClient.Initialize(appId, dotNetKey);
        }

        public void StoreNetwork(INeuralNetwork network, double eval)
        {
            var networkParseFormat = new ParseObject(_networkVersion);
            networkParseFormat["jsonNetwork"] = JsonConvert.SerializeObject(network.GetGenes());
            networkParseFormat["eval"] = eval;
            networkParseFormat.SaveAsync().Wait();
        }

        public ITrainingSession GetBestSession()
        {
            var task = ParseCloud.CallFunctionAsync<ParseObject>("bestNetwork", new Dictionary<string, object> { { "networkVersion", _networkVersion } });
            task.Wait();
            var result = task.Result;
            var networkGenes = JsonConvert.DeserializeObject<NeuralNetworkGene>((string)result["jsonNetwork"]);
            var network = NeuralNetworkFactory.GetInstance().Create(networkGenes);
            var session = new FakeTrainingSession(network, (double)result["eval"]);
            return session;
        }
    }
}
