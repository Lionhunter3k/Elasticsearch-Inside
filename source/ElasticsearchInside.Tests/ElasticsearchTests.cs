﻿using System;
using System.Threading.Tasks;
using ElasticsearchInside.Config;
using Nest;
using NUnit.Framework;

namespace ElasticsearchInside.Tests
{
    [TestFixture]
    public class ElasticsearchTests
    {
        [Test]
        public async Task Can_start()
        {
            using (var elasticsearch = await new Elasticsearch(i => i.SetPort(9194).EnableLogging()).Ready())
            {
                ////Arrange
                var client = new ElasticClient(new ConnectionSettings(elasticsearch.Url));

                ////Act
                var result = client.Ping();
                
                ////Assert
                Assert.That(result.IsValid);
            }
        }


        [Test]
        public void Can_start_sync()
        {
            using (var elasticsearch = new Elasticsearch(i => i.SetPort(9199).EnableLogging()).ReadySync())
            {
                ////Arrange
                var client = new ElasticClient(new ConnectionSettings(elasticsearch.Url));

                ////Act
                var result = client.Ping();

                ////Assert
                Assert.That(result.IsValid);
            }
        }
        [Test]
        public async Task Can_insert_data()
        {
            using (var elasticsearch = await new Elasticsearch(i => i.SetPort(9198).EnableLogging()).Ready())
            {
                ////Arrange
                var client = new ElasticClient(new ConnectionSettings(elasticsearch.Url));

                ////Act
                client.Index(new { id = "tester" }, i => i.Index("test-index").Type("test-type").Refresh(global::Elasticsearch.Net.Refresh.WaitFor));

                ////Assert
                var result = client.Get(DocumentPath<dynamic>.Id("tester"), i => i.Index("test-index").Type("test-type"));
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Found);
            }
        }

        [Test]
        public async Task Can_change_configuration()
        {
            using (var elasticsearch = await new Elasticsearch(c => c.SetPort(9197).EnableLogging().LogTo(Console.WriteLine)).Ready())
            {
                ////Arrange
                var client = new ElasticClient(new ConnectionSettings(elasticsearch.Url));

                ////Act
                var result = client.Ping();

                ////Assert
                Assert.That(result.IsValid);
                Assert.That(elasticsearch.Url.Port, Is.EqualTo(9197));
            }
        }
        
        [Test]
        public async Task Can_log_output()
        {
            var logged = false;
            using (var elasticsearch = new Elasticsearch(c => c.SetPort(9195).EnableLogging().LogTo(message => logged = true)))
            {
                await elasticsearch.Ready();

                ////Assert
                Assert.That(logged);
            }
        }

        [Test]
        public async Task Can_install_plugin()
        {
            using (var elasticsearch = await new Elasticsearch(c => c.SetPort(9196).EnableLogging().AddPlugin(new Plugin("analysis-icu"))).Ready())
            {
                ////Arrange
                var client = new ElasticClient(new ConnectionSettings(elasticsearch.Url));

                ////Act
                var result = await client.CatPluginsAsync();
                
                ////Assert
                Assert.That(result.Records.Count, Is.EqualTo(1));
            }
        }

        [Test]
        public async Task Folder_is_removed_after_dispose()
        {
            ////Arrange
            Settings settings;

            ////Act
            using (var elasticsearch = await new Elasticsearch(c => c.SetPort(9193).EnableLogging().LogTo(Console.WriteLine)).Ready())
                settings = (Settings)elasticsearch.Settings;


            ////Assert
            var folder = settings.RootFolder;
            folder.Refresh();

            Assert.That(!folder.Exists);
        }
    }
}
