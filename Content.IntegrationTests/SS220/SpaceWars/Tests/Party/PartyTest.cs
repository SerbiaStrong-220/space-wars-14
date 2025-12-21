// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt
using Content.Shared.SS220.SpaceWars.Party;
using Robust.Server.Player;
using Robust.Shared.Player;

namespace Content.IntegrationTests.SS220.SpaceWars.Tests.Party;

public sealed class PartyTest
{
    private readonly PoolSettings _poolSettings = new() { Connected = true, DummyTicker = true, };

    [Test]
    public async Task TestCreateChangeDisbandParty()
    {
        await using var pair = await PoolManager.GetServerClient(_poolSettings);
        var (server, client) = pair;
        await pair.RunTicksSync(5);

        var playerMng = server.ResolveDependency<IPlayerManager>();
        var serverPartyMng = server.ResolveDependency<Server.SS220.SpaceWars.Party.IPartyManager>();
        var clientPartyMng = client.ResolveDependency<Client.SS220.SpaceWars.Party.IPartyManager>();


        // Create party test
        ICommonSession session = default;
        Server.SS220.SpaceWars.Party.Party serverParty = default;
        await server.WaitAssertion(() =>
        {
            session = playerMng.GetSessionById(client.Session.UserId);
            serverPartyMng.CreateParty(session, out serverParty);

            Assert.Multiple(() =>
            {
                Assert.That(serverPartyMng.Parties, Has.Count.EqualTo(1));
                Assert.That(serverPartyMng.GetPartyById(serverParty.Id), Is.EqualTo(serverParty));
                Assert.That(serverPartyMng.GetPartyByHost(session), Is.EqualTo(serverParty));
                Assert.That(serverPartyMng.GetPartyByMember(session), Is.EqualTo(serverParty));
                Assert.That(serverPartyMng.PartyExist(serverParty), Is.True);
                Assert.That(serverParty.ContainsMember(session), Is.True);
                Assert.That(serverParty.Status, Is.EqualTo(PartyStatus.Running));
            });
        });
        await pair.RunTicksSync(5);

        var clientParty = clientPartyMng.LocalParty;
        Assert.That(clientParty, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(clientParty.Id, Is.EqualTo(serverParty.Id));
            Assert.That(clientParty.Members, Has.Count.EqualTo(1));
            Assert.That(clientParty.Host.UserId, Is.EqualTo(client.Session.UserId));
            Assert.That(clientParty.FindMember(client.Session.UserId), Is.Not.Null);
        });

        Assert.That(clientPartyMng.LocalMember, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(clientPartyMng.LocalMember.UserId, Is.EqualTo(client.Session.UserId));
            Assert.That(clientPartyMng.LocalMember.Role, Is.EqualTo(PartyMemberRole.Host));
        });

        Assert.That(clientPartyMng.IsLocalPartyHost, Is.True);


        // Add member test
        var dummy = await server.AddDummySession();
        await server.WaitAssertion(() =>
        {
            serverPartyMng.AddMember(serverParty, dummy);

            Assert.Multiple(() =>
            {
                Assert.That(serverPartyMng.GetPartyByMember(dummy), Is.EqualTo(serverParty));
                Assert.That(serverParty.Members, Has.Count.EqualTo(2));
            });
        });
        await pair.RunTicksSync(5);

        Assert.Multiple(() =>
        {
            Assert.That(clientParty.Members, Has.Count.EqualTo(2));
            Assert.That(clientParty.TryFindMember(dummy.UserId, out var dummyMember), Is.True);
            Assert.That(dummyMember.UserId, Is.EqualTo(dummy.UserId));
            Assert.That(dummyMember.Username, Is.EqualTo(dummy.Name));
            Assert.That(dummyMember.Role, Is.EqualTo(PartyMemberRole.Member));
        });


        // Set host test
        await server.WaitAssertion(() =>
        {
            serverPartyMng.SetHost(serverParty, dummy);

            Assert.Multiple(() =>
            {
                Assert.That(serverPartyMng.GetPartyByHost(session), Is.Null);
                Assert.That(serverPartyMng.GetPartyByHost(dummy), Is.EqualTo(serverParty));
                Assert.That(serverParty.Host.Session, Is.EqualTo(dummy));
                Assert.That(serverParty.Members, Has.Count.EqualTo(2));
            });
        });
        await pair.RunTicksSync(5);

        Assert.Multiple(() =>
        {
            Assert.That(clientPartyMng.IsLocalPartyHost, Is.False);
            Assert.That(clientPartyMng.LocalMember.Role, Is.EqualTo(PartyMemberRole.Member));
            Assert.That(clientParty.TryFindMember(dummy.UserId, out var dummyMember), Is.True);
            Assert.That(dummyMember.Role, Is.EqualTo(PartyMemberRole.Host));
        });


        // Remove member test
        await server.WaitAssertion(() =>
        {
            serverPartyMng.SetHost(serverParty, session);
            serverPartyMng.RemoveMember(serverParty, dummy);

            Assert.Multiple(() =>
            {
                Assert.That(serverPartyMng.GetPartyByMember(dummy), Is.Null);
                Assert.That(serverParty.Members, Has.Count.EqualTo(1));
            });
        });
        await pair.RunTicksSync(5);

        Assert.Multiple(() =>
        {
            Assert.That(clientParty.FindMember(dummy.UserId), Is.Null);
            Assert.That(clientParty.Members, Has.Count.EqualTo(1));
        });

        await server.RemoveDummySession(dummy);


        // Remove host test
        await server.WaitAssertion(() =>
        {
            serverPartyMng.AddMember(serverParty, dummy);
            serverPartyMng.SetHost(serverParty, dummy);
            Assert.Multiple(() =>
            {
                Assert.That(serverParty.IsHost(dummy), Is.True);
                Assert.That(serverParty.IsHost(session), Is.False);
            });

            serverPartyMng.RemoveMember(serverParty, dummy);

            Assert.Multiple(() =>
            {
                Assert.That(serverPartyMng.PartyExist(serverParty), Is.True);
                Assert.That(serverParty.IsHost(session));
                Assert.That(serverPartyMng.GetPartyByHost(session), Is.EqualTo(serverParty));

                Assert.That(serverPartyMng.GetPartyByHost(dummy), Is.Null);
                Assert.That(serverPartyMng.GetPartyByMember(dummy), Is.Null);
            });
        });
        await pair.RunTicksSync(5);
        Assert.Multiple(() =>
        {
            Assert.That(clientPartyMng.LocalParty, Is.Not.Null);
            Assert.That(clientPartyMng.IsLocalPartyHost, Is.True);
        });


        // Disband party test
        await server.WaitAssertion(() =>
        {
            serverPartyMng.DisbandParty(serverParty);

            Assert.Multiple(() =>
            {
                Assert.That(serverPartyMng.Parties, Has.Count.Zero);
                Assert.That(serverPartyMng.GetPartyById(serverParty.Id), Is.Null);
                Assert.That(serverPartyMng.GetPartyByHost(session), Is.Null);
                Assert.That(serverPartyMng.GetPartyByMember(session), Is.Null);
                Assert.That(serverPartyMng.PartyExist(serverParty), Is.False);
                Assert.That(serverParty.Status, Is.EqualTo(PartyStatus.Disbanded));
            });
        });
        await pair.RunTicksSync(5);

        Assert.Multiple(() =>
        {
            Assert.That(clientPartyMng.LocalParty, Is.Null);
            Assert.That(clientPartyMng.LocalMember, Is.Null);
        });

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task TestPartyInvitesFromHostPerspective()
    {
        await using var pair = await PoolManager.GetServerClient(_poolSettings);
        var (server, client) = pair;
        await pair.RunTicksSync(5);

        var playerMng = server.ResolveDependency<IPlayerManager>();
        var serverPartyMng = server.ResolveDependency<Server.SS220.SpaceWars.Party.IPartyManager>();
        var clientPartyMng = client.ResolveDependency<Client.SS220.SpaceWars.Party.IPartyManager>();

        var dummy = await server.AddDummySession();

        ICommonSession session = default;
        Server.SS220.SpaceWars.Party.Party serverParty = default;
        await server.WaitPost(() =>
        {
            session = playerMng.GetSessionById(client.Session.UserId);
            serverPartyMng.CreateParty(session, out serverParty);
        });
        await pair.RunTicksSync(5);


        // Client creates invite test
        await client.WaitPost(() => clientPartyMng.InviteUserRequest(dummy.Name));
        await pair.RunTicksSync(5);

        Server.SS220.SpaceWars.Party.PartyInvite serverInvite = default;
        uint inviteId = default!;
        await server.WaitAssertion(() =>
        {
            Assert.That(serverPartyMng.Invites, Has.Count.EqualTo(1));
            serverPartyMng.TryGetInvite(serverParty, dummy, out serverInvite);

            Assert.That(serverInvite, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(serverInvite.Party, Is.EqualTo(serverParty));
                Assert.That(serverInvite.Target, Is.EqualTo(dummy));
                Assert.That(serverInvite.Status, Is.EqualTo(PartyInviteStatus.Sended));
            });

            inviteId = serverInvite.Id;
        });

        await client.WaitAssertion(() =>
        {
            Assert.That(clientPartyMng.LocalParty?.Invites, Has.Count.EqualTo(1));

            Client.SS220.SpaceWars.Party.PartyInvite invite = default;
            clientPartyMng.LocalParty?.TryGetInvite(inviteId, out invite);

            Assert.That(invite, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(invite.PartyId, Is.EqualTo(serverInvite.Party.Id));
                Assert.That(invite.Target, Is.EqualTo(dummy.UserId));
                Assert.That(invite.TargetName, Is.EqualTo(dummy.Name));
                Assert.That(invite.Status, Is.EqualTo(serverInvite.Status));
            });
        });


        // Server updats invite status test
        await server.WaitAssertion(() => serverPartyMng.SetInviteStatus(serverInvite, PartyInviteStatus.Denied));
        await pair.RunTicksSync(5);

        await server.WaitAssertion(() =>
        {
            Assert.That(serverPartyMng.Invites, Has.Count.EqualTo(1));
            serverPartyMng.TryGetInvite(serverParty, dummy, out serverInvite);

            Assert.That(serverInvite, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(serverInvite.Party, Is.EqualTo(serverParty));
                Assert.That(serverInvite.Target, Is.EqualTo(dummy));
                Assert.That(serverInvite.Status, Is.EqualTo(PartyInviteStatus.Denied));
            });
        });

        await client.WaitAssertion(() =>
        {
            Assert.That(clientPartyMng.LocalParty?.Invites, Has.Count.EqualTo(1));

            Client.SS220.SpaceWars.Party.PartyInvite invite = default;
            clientPartyMng.LocalParty?.TryGetInvite(inviteId, out invite);

            Assert.That(invite, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(invite.PartyId, Is.EqualTo(serverInvite.Party.Id));
                Assert.That(invite.Target, Is.EqualTo(dummy.UserId));
                Assert.That(invite.TargetName, Is.EqualTo(dummy.Name));
                Assert.That(invite.Status, Is.EqualTo(serverInvite.Status));
            });
        });


        // Client deletes invite test
        await client.WaitAssertion(() => clientPartyMng.DeleteLocalPartyInviteRequest(inviteId));
        await pair.RunTicksSync(5);

        await server.WaitAssertion(() =>
        {
            Assert.That(serverPartyMng.Invites, Has.Count.EqualTo(0));
            serverPartyMng.TryGetInvite(serverParty, dummy, out serverInvite);

            Assert.That(serverInvite, Is.Null);
        });

        await client.WaitAssertion(() =>
        {
            Assert.That(clientPartyMng.LocalParty?.Invites, Has.Count.EqualTo(0));

            Client.SS220.SpaceWars.Party.PartyInvite invite = default;
            clientPartyMng.LocalParty?.TryGetInvite(inviteId, out invite);

            Assert.That(invite, Is.Null);
        });

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task TestPartyInvitesFromReceiverPerspective()
    {
        await using var pair = await PoolManager.GetServerClient(_poolSettings);
        var (server, client) = pair;
        await pair.RunTicksSync(5);

        var playerMng = server.ResolveDependency<IPlayerManager>();
        var serverPartyMng = server.ResolveDependency<Server.SS220.SpaceWars.Party.IPartyManager>();
        var clientPartyMng = client.ResolveDependency<Client.SS220.SpaceWars.Party.IPartyManager>();

        var dummy = await server.AddDummySession();

        ICommonSession session = default;
        Server.SS220.SpaceWars.Party.Party serverParty = default;
        await server.WaitPost(() =>
        {
            session = playerMng.GetSessionById(client.Session.UserId);
            serverPartyMng.CreateParty(dummy, out serverParty);
        });
        await pair.RunTicksSync(5);


        // Server creates invite test
        Server.SS220.SpaceWars.Party.PartyInvite serverInvite = default;
        uint inviteId = default;

        await ServerCreateNewInvite();
        await client.WaitAssertion(() =>
        {
            Assert.That(clientPartyMng.ReceivedInvites, Has.Count.EqualTo(1));
            clientPartyMng.TryGetReceivedInvite(inviteId, out var invite);

            Assert.That(invite, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(invite.PartyId, Is.EqualTo(serverInvite.Party.Id));
                Assert.That(invite.SenderName, Is.EqualTo(dummy.Name));
                Assert.That(invite.Status, Is.EqualTo(serverInvite.Status));
            });
        });


        // Server delete invite test
        await server.WaitAssertion(() => serverPartyMng.DeleteInvite(serverInvite));
        await pair.RunTicksSync(5);

        await server.WaitAssertion(() =>
        {
            Assert.That(serverPartyMng.Invites, Has.Count.EqualTo(0));
            serverPartyMng.TryGetInvite(serverParty, dummy, out serverInvite);

            Assert.That(serverInvite, Is.Null);
        });

        await client.WaitAssertion(() =>
        {
            Assert.That(clientPartyMng.ReceivedInvites, Has.Count.EqualTo(0));

            clientPartyMng.TryGetReceivedInvite(inviteId, out var invite);
            Assert.That(invite, Is.Null);
        });


        // Client accepts invite test
        await ServerCreateNewInvite();
        await client.WaitPost(() => clientPartyMng.AcceptReceivedInviteRequest(inviteId));
        await pair.RunTicksSync(5);

        await server.WaitAssertion(() =>
        {
            Assert.That(serverParty.Invites, Has.Count.EqualTo(0));
            serverPartyMng.TryGetInvite(serverParty, dummy, out serverInvite);

            Assert.That(serverInvite, Is.Null);

            Assert.Multiple(() =>
            {
                Assert.That(serverPartyMng.GetPartyByMember(session), Is.EqualTo(serverParty));
                Assert.That(serverParty.Members, Has.Count.EqualTo(2));
            });
        });

        await client.WaitAssertion(() =>
        {
            Assert.That(clientPartyMng.ReceivedInvites, Has.Count.EqualTo(0));

            clientPartyMng.TryGetReceivedInvite(inviteId, out var invite);
            Assert.Multiple(() =>
            {
                Assert.That(invite, Is.Null);
                Assert.That(clientPartyMng.LocalParty.Id, Is.EqualTo(serverParty.Id));
            });
        });

        await server.WaitAssertion(() => serverPartyMng.RemoveMember(serverParty, session));
        await pair.RunTicksSync(5);


        // Client denies invite test
        await ServerCreateNewInvite();
        await client.WaitPost(() => clientPartyMng.DenyReceivedInviteRequest(serverInvite.Id));
        await pair.RunTicksSync(5);

        await server.WaitAssertion(() =>
        {
            Assert.That(serverParty.Invites, Has.Count.EqualTo(1));
            serverPartyMng.TryGetInvite(serverParty, session, out serverInvite);

            Assert.That(serverInvite, Is.Not.Null);
            Assert.That(serverInvite.Status, Is.EqualTo(PartyInviteStatus.Denied));
        });

        await client.WaitAssertion(() =>
        {
            Assert.That(clientPartyMng.ReceivedInvites, Has.Count.EqualTo(0));

            clientPartyMng.TryGetReceivedInvite(inviteId, out var invite);
            Assert.That(invite, Is.Null);
        });

        // Check invite deletion after deny
        await pair.RunSeconds((float)Server.SS220.SpaceWars.Party.PartyManager.DeleteAfterDenyDelay.TotalSeconds);

        await server.WaitAssertion(() =>
        {
            Assert.That(serverPartyMng.Invites, Has.Count.EqualTo(0));
            serverPartyMng.TryGetInvite(serverParty, dummy, out serverInvite);

            Assert.That(serverInvite, Is.Null);
        });

        await client.WaitAssertion(() =>
        {
            Assert.That(clientPartyMng.ReceivedInvites, Has.Count.EqualTo(0));

            clientPartyMng.TryGetReceivedInvite(inviteId, out var invite);
            Assert.That(invite, Is.Null);
        });


        await pair.CleanReturnAsync();

        async Task ServerCreateNewInvite()
        {
            await server.WaitPost(() => serverPartyMng.CreateInvite(serverParty, session, out _));
            await pair.RunTicksSync(5);

            await server.WaitAssertion(() =>
            {
                Assert.That(serverPartyMng.Invites, Has.Count.EqualTo(1));
                serverPartyMng.TryGetInvite(serverParty, session, out serverInvite);

                Assert.That(serverInvite, Is.Not.Null);
                Assert.Multiple(() =>
                {
                    Assert.That(serverInvite.Party, Is.EqualTo(serverParty));
                    Assert.That(serverInvite.Target, Is.EqualTo(session));
                    Assert.That(serverInvite.Status, Is.EqualTo(PartyInviteStatus.Sended));
                });
            });

            inviteId = serverInvite.Id;
        }
    }
}
