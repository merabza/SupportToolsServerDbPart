﻿using System.Collections.Generic;
using System.Linq;
using DatabaseToolsShared;
using SupportToolsServerDb.Models;
using SupportToolsServerDbDataSeeding.Models;

namespace SupportToolsServerDbDataSeeding.Seeders;

public class StsApiKeysSeeder : DataSeeder<ApiKeyByRemoteIpAddress, ApiKeyByRemoteIpAddressSeedarModel>
{
    private readonly string _secretDataFolder;

    // ReSharper disable once ConvertToPrimaryConstructor
    public StsApiKeysSeeder(string secretDataFolder, IStsDataSeederRepository repo) : base(secretDataFolder, repo,
        ESeedDataType.OnlyRules)
    {
        _secretDataFolder = secretDataFolder;
        //Repo = repo;
    }

    //private IStsDataSeederRepository Repo { get; }

    protected override bool AdditionalCheck(List<ApiKeyByRemoteIpAddressSeedarModel> jsonData, List<ApiKeyByRemoteIpAddress> savedData)
    {
        var existingApiKeys = Repo.GetAll<ApiKeyByRemoteIpAddress>();

        var userToCreate = GetApiKeyModels().Select(apiKeyModel => new
        {
            apiKeyModel,
            existingApiKeyByIpAddress =
                existingApiKeys.SingleOrDefault(sd =>
                    sd.ApiKey == apiKeyModel.ApiKey && sd.RemoteIpAddress == apiKeyModel.RemoteIpAddress)
        }).Where(w => w.existingApiKeyByIpAddress == null).Select(s => s.apiKeyModel).ToList();

        return Repo.CreateEntities(userToCreate.Select(x =>
            new ApiKeyByRemoteIpAddress { ApiKey = x.ApiKey, RemoteIpAddress = x.RemoteIpAddress }).ToList());
    }

    //private bool CreateApiKey(ApiKeyModel apiKeyModel)
    //{
    //    //1. შევქმნათ ახალი მომხმარებელი
    //    var apiKey = new ApiKeyByRemoteIpAddress
    //    {
    //        ApiKey = apiKeyModel.ApiKey, RemoteIpAddress = apiKeyModel.RemoteIpAddress
    //    };
    //    var result = Repo.CreateApiKey(apiKey);
    //    //თუ ახალი მომხმარებლის შექმნისას წარმოიშვა პრობლემა, ვჩერდებით
    //    if (result)
    //        return true;

    //    throw new Exception(
    //        $"apiKey {apiKeyModel.ApiKey} with Ip address {apiKeyModel.RemoteIpAddress} can not be created.");
    //}

    private List<ApiKeyModel> GetApiKeyModels()
    {
        return [.. LoadFromJsonFile<ApiKeyModel>(_secretDataFolder, "ApiKeysByRemoteIpAddresses.json")];
    }
}