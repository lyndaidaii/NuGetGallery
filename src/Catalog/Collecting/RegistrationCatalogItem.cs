﻿using Newtonsoft.Json.Linq;
using NuGet.Services.Metadata.Catalog.Helpers;
using NuGet.Services.Metadata.Catalog.Maintenance;
using NuGet.Services.Metadata.Catalog.Persistence;
using System;
using System.Linq;
using VDS.RDF;
using VDS.RDF.Query;

namespace NuGet.Services.Metadata.Catalog.Collecting
{
    public class RegistrationCatalogItem : CatalogItem
    {
        Uri _catalogUri;
        IGraph _catalogItem;
        Uri _itemAddress;
        Uri _packageContentBaseAddress;
        Uri _packageContentAddress;
        Uri _registrationBaseAddress;

        public RegistrationCatalogItem(Uri catalogUri, IGraph catalogItem, Uri packageContentBaseAddress, Uri registrationBaseAddress)
        {
            _catalogUri = catalogUri;
            _catalogItem = catalogItem;
            _packageContentBaseAddress = packageContentBaseAddress;
            _registrationBaseAddress = registrationBaseAddress;
        }

        public override StorageContent CreateContent(CatalogContext context)
        {
            IGraph graph = new Graph();
            INode subject = graph.CreateUriNode(GetItemAddress());
            graph.Assert(subject, graph.CreateUriNode(Schema.Predicates.Type), graph.CreateUriNode(Schema.DataTypes.Package));
            graph.Assert(subject, graph.CreateUriNode(Schema.Predicates.CatalogEntry), graph.CreateUriNode(_catalogUri));
            graph.Assert(subject, graph.CreateUriNode(Schema.Predicates.PackageContent), graph.CreateUriNode(GetPackageContentAddress()));
            JObject frame = context.GetJsonLdContext("context.Package.json", Schema.DataTypes.Package);
            return new StringStorageContent(Utils.CreateJson(graph, frame), "application/json", "no-store");
        }

        public override Uri GetItemType()
        {
            return Schema.DataTypes.Package;
        }

        public override Uri GetItemAddress()
        {
            if (_itemAddress == null)
            {
                INode subject = _catalogItem.CreateUriNode(_catalogUri);
                string version = _catalogItem.GetTriplesWithSubjectPredicate(subject, _catalogItem.CreateUriNode(Schema.Predicates.Version)).FirstOrDefault().Object.ToString().ToLowerInvariant();
                _itemAddress = new Uri(BaseAddress, version + ".json");
            }

            return _itemAddress;
        }

        Uri GetPackageContentAddress()
        {
            if (_packageContentAddress == null)
            {
                INode subject = _catalogItem.CreateUriNode(_catalogUri);
                string id = _catalogItem.GetTriplesWithSubjectPredicate(subject, _catalogItem.CreateUriNode(Schema.Predicates.Id)).FirstOrDefault().Object.ToString().ToLowerInvariant();
                string version = _catalogItem.GetTriplesWithSubjectPredicate(subject, _catalogItem.CreateUriNode(Schema.Predicates.Version)).FirstOrDefault().Object.ToString().ToLowerInvariant();
                string path = string.Format("packages/{0}.{1}.nupkg", id.ToLowerInvariant(), version.ToLowerInvariant());
                _packageContentAddress = new Uri(_packageContentBaseAddress, path);
            }

            return _packageContentAddress;
        }

        public override IGraph CreatePageContent(CatalogContext context)
        {
            try
            {
                IGraph content;

                using (TripleStore store = new TripleStore())
                {
                    store.Add(_catalogItem, true);

                    SparqlParameterizedString sparql = new SparqlParameterizedString();
                    sparql.CommandText = Utils.GetResource("sparql.ConstructPackagePageContentGraph.rq");

                    sparql.SetUri("package", GetItemAddress());
                    sparql.SetUri("catalogPackage", _catalogUri);
                    sparql.SetUri("baseAddress", BaseAddress);
                    sparql.SetUri("packageContent", GetPackageContentAddress());
                    sparql.SetUri("registrationBaseAddress", _registrationBaseAddress);

                    content = SparqlHelpers.Construct(store, sparql.ToString());
                }

                return content;
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("Exception processing catalog item {0}", _catalogUri), e);
            }
        }
    }
}
