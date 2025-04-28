﻿using Akiles.ApiClient.MemberGroups;

namespace Akiles.ApiClient.Tests;

public class MemberGroupsTests(ApiFixture fixture) : IClassFixture<ApiFixture>
{
    private readonly IAkilesApiClient _client = fixture.Client;

    [Fact]
    public async Task CanListMemberGroups()
    {
        // Given

        // When
        var groups = await _client.MemberGroups.ListMemberGroupsAsync().ToListAsync();

        // Then
        Assert.NotEmpty(groups);
    }

    [Fact]
    public async Task CanGetMemberGroup()
    {
        // Given

        // When
        var group = await _client.MemberGroups.GetMemberGroupAsync("mg_41hmmbk2u95nk44jxdkh");

        // Then
        Assert.Equal("TEST", group.Name);
    }

    [Fact]
    public async Task CanUpdateMemberGroup()
    {
        // Given

        // When
        var group = await _client.MemberGroups.EditMemberGroupAsync(
            "mg_41hmmbk2u95nk44jxdkh",
            new()
            {
                Permissions =
                [
                    new MemberGroupPermissionRule() { AccessMethods = new() { Card = true } }
                ]
            }
        );

        // Then
        var permission = Assert.Single(group.Permissions);
        Assert.True(permission.AccessMethods!.Card);
    }
}
