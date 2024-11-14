using Google.Apis.Walletobjects.v1.Data;
using System.Text.Json;
using WalliCardsNet.API.Models;
using System.Reflection.Metadata.Ecma335;

namespace WalliCardsNet.API.Builders
{
    public class GooglePassBuilder
    {
        private readonly GenericClass _genericClass;
        private readonly GenericObject _genericObject;
        private readonly string _issuerId;
        private readonly string _devTunnel;

        public GooglePassBuilder()
        {
            _genericClass = new GenericClass();
            _genericObject = new GenericObject();
            _issuerId = Environment.GetEnvironmentVariable("GOOGLE-WALLET-ISSUER-ID") ?? throw new NullReferenceException("Not able to load IssuerId");
            _devTunnel = Environment.GetEnvironmentVariable("DEV_TUNNEL_ADDRESS") ?? throw new NullReferenceException("Not able to load Dev tunnel uri");

        }

        #region Generic Class related methods

        public GooglePassBuilder ClassWithBasicInfo(Guid businessProfileId)
        {
            _genericClass.Id = $"{_issuerId}.{businessProfileId}";
            _genericClass.MultipleDevicesAndHoldersAllowedStatus = "ONE_USER_ALL_DEVICES";
            _genericClass.CallbackOptions = new CallbackOptions
            {
                Url = _devTunnel
            };
            

            return this;
        }

        public GooglePassBuilder WithLayoutDetails(string fieldJson)
        {
            try
            {
                var rows = JsonSerializer.Deserialize<List<List<string>>>(fieldJson);

                if (rows == null || !rows.Any())
                {
                    throw new ArgumentException("Invalid or empty field configuration");
                }

                var cardRowTemplateInfos = new List<CardRowTemplateInfo>();

                var firstRowFields = rows[0];
                var firstRowTemplateInfo = CreateCardRowTemplateInfo(firstRowFields);
                cardRowTemplateInfos.Add(firstRowTemplateInfo);

                if (rows.Count > 1)
                {
                    var secondRowFields = rows[1];
                    var secondRowTemplateInfo = CreateCardRowTemplateInfo(secondRowFields);
                    cardRowTemplateInfos.Add(secondRowTemplateInfo);
                }

                _genericClass.ClassTemplateInfo = new ClassTemplateInfo
                {
                    CardTemplateOverride = new CardTemplateOverride
                    {
                        CardRowTemplateInfos = cardRowTemplateInfos
                    }
                };

                return this;
            }
            catch (JsonException ex)
            {
                throw new ArgumentException("Failed to parse JSON", ex);
            }
        }

        public GenericClass BuildClass()
        {
            return _genericClass;
        }

        public GenericClass BuildClassFromTemplate(BusinessProfile profile)
        {
            var builder = new GooglePassBuilder();

            return builder
                .ClassWithBasicInfo(profile.Id)
                .WithLayoutDetails(profile.GoogleTemplate.FieldsJson)
                //.WithMessages(template.Messages)
                .BuildClass();
        }

        #endregion

        #region Generic Object related methods

        public GooglePassBuilder ObjectWithBasicInfo(Guid businessProfileId, Guid customerId, string? hexBackgroundColor, string cardTitle, string header)
        {
            _genericObject.GenericType = "GENERIC_OTHER";
            _genericObject.Id = $"{_issuerId}.{customerId}";
            _genericObject.ClassId = $"{_issuerId}.{businessProfileId}";

            _genericObject.CardTitle = CreateLocalizedString(cardTitle);
            _genericObject.Header = CreateLocalizedString(header);

            if (hexBackgroundColor != null)
            {
                _genericObject.HexBackgroundColor = hexBackgroundColor;
            }

            return this;
        }

        public GooglePassBuilder WithImageInfo(string? logoUri, string? wideLogoUri, string? heroUri)
        {
            //if (logoUri != null && wideLogoUri == null)
            if (!string.IsNullOrEmpty(logoUri) && string.IsNullOrEmpty(wideLogoUri))
            {
                _genericObject.Logo = new Image
                {
                    SourceUri = new ImageUri
                    {
                        Uri = logoUri,
                    },
                    ContentDescription = CreateLocalizedString("LOGO_IMAGE_DESCRIPTION"),
                };
            }

            //if (wideLogoUri != null)
            if (!string.IsNullOrEmpty(wideLogoUri))
            {
                _genericObject.WideLogo = new Image
                {
                    SourceUri = new ImageUri
                    {
                        Uri = wideLogoUri,
                    },
                    ContentDescription = CreateLocalizedString("WIDELOGO_IMAGE_DESCRIPTION"),
                };
            }

            if(!string.IsNullOrEmpty(heroUri))
            {
                _genericObject.HeroImage = new Image
                {
                    SourceUri = new ImageUri
                    {
                        Uri = heroUri
                    },
                    ContentDescription = CreateLocalizedString("HERO_IMAGE_DESCRIPTION"),
                };
            }
            return this;
        }

        public GooglePassBuilder WithTextModulesData(string? fieldsJson, Dictionary<string, string>? customerDetails)
        {
            if(!string.IsNullOrEmpty(fieldsJson) && customerDetails != null) //TODO: CustomerDetails kontroll måste ändras till att se om det finns något innehåll, ej null.
            {
                var fields = new List<string>();
                var rows = JsonSerializer.Deserialize<List<List<string>>>(fieldsJson);

                foreach(var row in rows!)
                {
                    foreach(var field in row)
                    {
                        fields.Add(field);
                    }
                }

                _genericObject.TextModulesData = fields.Select(f => new TextModuleData
                {
                    Header = f.ToUpper(),
                    Body = customerDetails.TryGetValue(f, out string? value) ? value : "No data",
                    Id = f.ToLower()
                }).ToList();
            }
            return this;
        }

        //public GooglePassBuilder WithLinksModuleData(List<LinksModule> links)
        //{
        //    if(links.Count > 0)
        //    {
        //        _genericObject.LinksModuleData = new LinksModuleData
        //        {
        //            Uris = links.Select(link => new Google.Apis.Walletobjects.v1.Data.Uri
        //            {
        //                UriValue = link.Uri,
        //                Description = link.Description,
        //                Id = link.Id
        //            }).ToList()
        //        };
        //    }

        //    return this;
        //}

        //public GooglePassBuilder WithImageModulesData(List<ImageModule> images)
        //{
        //    if(images.Count > 0)
        //    {
        //        _genericObject.ImageModulesData = images.Select(img => new ImageModuleData 
        //        {
        //            MainImage = new Image
        //            {
        //                SourceUri = new ImageUri 
        //                { 
        //                    Uri = img.Uri 
        //                },
        //                ContentDescription = CreateLocalizedString(img.Description),
        //            },
        //            Id = img.Id
        //        }).ToList();
        //    }

        //    return this;
        //}

        public GenericObject BuildObject()
        {
            return _genericObject;
        }
        public GenericObject BuildObjectFromTemplate(BusinessProfile profile, Customer customer) //TODO: customerDetails: count = 0 här!
        {
            var builder = new GooglePassBuilder();

            return builder
                .ObjectWithBasicInfo(profile.Id, customer.Id, profile.GoogleTemplate!.HexBackgroundColor, profile.GoogleTemplate!.CardTitle, profile.GoogleTemplate!.Header)
                .WithImageInfo(profile.GoogleTemplate.LogoUri, profile.GoogleTemplate.WideLogoUri, profile.GoogleTemplate.HeroImageUri)
                .WithTextModulesData(profile.GoogleTemplate.FieldsJson, customer.CustomerDetails)
                //.WithLinksModuleData(template.LinksModuleData)
                //.WithImageModulesData(template.ImageNodulesData)
                //.WithMessages(template.Messages)
                .BuildObject();
        }

        #endregion

        #region Support methods and classes

        //public GooglePassBuilder WithMessages(List<PassMessage> messages)
        //{
        //    _genericObject.Messages = messages.Select(msg => new Message
        //    {
        //        Id = msg.Id,
        //        Kind = "walletobjects#walletObjectMessage",
        //        Header = msg.Header,
        //        Body = msg.Body,
        //        DisplayInterval = new TimeInterval
        //        {
        //            Kind = "walletobjects#timeInterval",
        //            Start = new Google.Apis.Walletobjects.v1.Data.DateTime
        //            {
        //                Date = msg.Start.ToString("yyyy-MM-ddTHH:mm:ss.ff'Z'"),
        //            },
        //            End = new Google.Apis.Walletobjects.v1.Data.DateTime
        //            {
        //                Date = msg.End.ToString("yyyy-MM-ddTHH:mm:ss.ff'Z'"),
        //            }
        //        },
        //        MessageType = msg.MessageType,
        //        LocalizedHeader = CreateLocalizedString(msg.Header),
        //        LocalizedBody = CreateLocalizedString(msg.Body),

        //    }).ToList();
        //    return this;
        //}

        private LocalizedString CreateLocalizedString(string value, string language = "en-US")
        {
            return new LocalizedString
            {
                DefaultValue = new TranslatedString
                {
                    Language = language,
                    Value = value
                }
            };
        }

        private CardRowTemplateInfo CreateCardRowTemplateInfo(List<string> fields)
        {
            var cardRowTemplateInfo = new CardRowTemplateInfo();

            switch (fields.Count)
            {
                case 0:
                    return cardRowTemplateInfo;

                case 1:
                    cardRowTemplateInfo.OneItem = new CardRowOneItem
                    {
                        Item = new TemplateItem
                        {
                            FirstValue = new FieldSelector
                            {
                                Fields = new List<FieldReference>
                                {
                                    new FieldReference
                                    {
                                        FieldPath = $"object.textModulesData[\'{fields[0].ToLower()}\']"
                                    }
                                }
                            },
                        }
                    };
                    break;

                case 2:
                    cardRowTemplateInfo.TwoItems = new CardRowTwoItems
                    {
                        StartItem = new TemplateItem
                        {
                            FirstValue = new FieldSelector
                            {
                                Fields = new List<FieldReference>
                                {
                                    new FieldReference
                                    {
                                        FieldPath = $"object.textModulesData[\'{fields[0].ToLower()}\']"
                                    }
                                }
                            }
                        },
                        EndItem = new TemplateItem
                        {
                            SecondValue = new FieldSelector
                            {
                                Fields = new List<FieldReference> 
                                {
                                    new FieldReference
                                    {
                                        FieldPath = $"object.textModulesData[\'{fields[1].ToLower()}\']"
                                    }
                                }
                            }
                        }
                    };
                    break;

                case 3:
                    cardRowTemplateInfo.ThreeItems = new CardRowThreeItems
                    {
                        StartItem = new TemplateItem
                        {
                            FirstValue = new FieldSelector
                            {
                                Fields = new List<FieldReference>
                                { 
                                    new FieldReference
                                    {
                                        FieldPath = $"object.textModulesData[\'{fields[0].ToLower()}\']"
                                    }
                                }
                            }
                        },
                        MiddleItem = new TemplateItem
                        {
                            FirstValue = new FieldSelector
                            {
                                Fields = new List<FieldReference>
                                {
                                    new FieldReference
                                    {
                                        FieldPath = $"object.textModulesData[\'{fields[1].ToLower()}\']"
                                    }
                                }
                            }
                        },
                        EndItem = new TemplateItem
                        {
                            FirstValue = new FieldSelector
                            {
                                Fields = new List<FieldReference> 
                                {
                                    new FieldReference
                                    {
                                        FieldPath = $"object.textModulesData[\'{fields[2].ToLower()}\']"
                                    }
                                }
                            }
                        }
                    };
                    break;

                default:

                    throw new ArgumentException($"Incorrect field amount per row (actual: {fields.Count}, max: 3)");
            }

            return cardRowTemplateInfo;
        }

        #endregion
    }
}
