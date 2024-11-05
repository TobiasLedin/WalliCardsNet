using WalliCardsNet.ClassLibrary.BusinessProfile;
using Google.Apis.Walletobjects.v1.Data;
using static WalliCardsNet.ClassLibrary.BusinessProfile.GooglePassTemplateDTO;

namespace WalliCardsNet.API.Builders
{
    public class GooglePassBuilder
    {
        private readonly GenericClass _genericClass;
        private readonly GenericObject _genericObject;

        public GooglePassBuilder()
        {
            _genericClass = new GenericClass();
            _genericObject = new GenericObject();
        }

        public GooglePassBuilder WithBasicInfo(string objectId, string classId, string state, string cardTitle, string header, string hexBackgroundColor)
        {
            _genericObject.GenericType = "GENERIC_OTHER";
            _genericObject.Id = objectId;
            _genericObject.ClassId = classId;
            _genericObject.State = state;
            _genericObject.HexBackgroundColor = hexBackgroundColor;
            _genericObject.CardTitle = CreateLocalizedString(cardTitle);
            _genericObject.Header = CreateLocalizedString(header);
            
            return this;
        }

        public GooglePassBuilder WithImageInfo(string? logoUri, string? logoDescription, string? wideLogoUri, string? wideLogoDescription, string? heroUri, string? heroDescription)
        {
            if (logoUri != null && logoDescription != null && wideLogoUri == null)
            {
                _genericObject.Logo = new Image
                {
                    SourceUri = new ImageUri
                    {
                        Uri = logoUri,
                    },
                    ContentDescription = CreateLocalizedString(logoDescription),
                };
            }

            if (wideLogoUri != null && wideLogoDescription != null)
            {
                _genericObject.WideLogo = new Image
                {
                    SourceUri = new ImageUri
                    {
                        Uri = wideLogoUri,
                    },
                    ContentDescription = CreateLocalizedString(wideLogoDescription),
                };
            }

            if(heroUri != null && heroDescription != null)
            {
                _genericObject.HeroImage = new Image
                {
                    SourceUri = new ImageUri
                    {
                        Uri = heroUri
                    },
                    ContentDescription = CreateLocalizedString(heroDescription),
                };
            }
            return this;
        }

        public GooglePassBuilder WithTextModulesData(List<TextModuleDTO> textModules)
        {
            if(textModules.Count > 0)
            {
                _genericObject.TextModulesData = textModules.Select(tm => new TextModuleData
                {
                    Header = tm.Header,
                    Body = tm.Body,
                    Id = tm.Id
                }).ToList();
            }
            return this;
        }

        public GooglePassBuilder WithLinksModuleData(List<LinksModuleDTO> links)
        {
            if(links.Count > 0)
            {
                _genericObject.LinksModuleData = new LinksModuleData
                {
                    Uris = links.Select(link => new Google.Apis.Walletobjects.v1.Data.Uri
                    {
                        UriValue = link.Uri,
                        Description = link.Description,
                        Id = link.Id
                    }).ToList()
                };
            }

            return this;
        }

        public GooglePassBuilder WithImageModulesData(List<ImageModuleDTO> images)
        {
            if(images.Count > 0)
            {
                _genericObject.ImageModulesData = images.Select(img => new ImageModuleData 
                {
                    MainImage = new Image
                    {
                        SourceUri = new ImageUri 
                        { 
                            Uri = img.Uri 
                        },
                        ContentDescription = CreateLocalizedString(img.Description),
                    },
                    Id = img.Id
                }).ToList();
            }

            return this;
        }

        public GooglePassBuilder WithMessages(List<MessageDTO> messages)
        {
            _genericObject.Messages = messages.Select(msg => new Message
            {
                Id = msg.Id,
                Kind = "walletobjects#walletObjectMessage",
                Header = msg.Header,
                Body = msg.Body,
                DisplayInterval = new TimeInterval
                {
                    Kind = "walletobjects#timeInterval",
                    Start = new Google.Apis.Walletobjects.v1.Data.DateTime
                    {
                        Date = msg.Start.ToString("yyyy-MM-ddTHH:mm:ss.ff'Z'"),
                    },
                    End = new Google.Apis.Walletobjects.v1.Data.DateTime
                    {
                        Date = msg.End.ToString("yyyy-MM-ddTHH:mm:ss.ff'Z'"),
                    }
                },
                MessageType = msg.MessageType,
                LocalizedHeader = CreateLocalizedString(msg.Header),
                LocalizedBody = CreateLocalizedString(msg.Body),

            }).ToList();
            return this;
        }

        //public GooglePassBuilder ClassCreation()
        //{
        //    _genericClass.Id = "issuerId + businessId";             // TODO: Fixa
        //    _genericClass.ClassTemplateInfo = new ClassTemplateInfo
        //    {
        //       new CardTemplateOverride
        //       {
        //           new CardRowTemplateInfos
        //           {

        //           }
        //       }
        //    };
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

        public GenericObject Build()
        {
            return _genericObject;
        }

        public GenericObject CreateObjectFromDTO(GooglePassTemplateDTO dto)
        {
            var builder = new GooglePassBuilder();

            return builder
                .WithBasicInfo(dto.ObjectId, dto.ClassId, dto.State, dto.CardTitle, dto.Header, dto.HexBackgroundColor)
                .WithImageInfo(dto.LogoImageUri, dto.LogoImageDescription, dto.WideLogoImageUri, dto.WideLogoImageDescription, dto.HeroImageUri, dto.HeroImageDescription)
                .WithTextModulesData(dto.TextModulesData)
                .WithLinksModuleData(dto.LinksModuleData)
                .WithImageModulesData(dto.ImageNodulesData)
                .WithMessages(dto.Messages)
                .Build();
        }
    }

}
