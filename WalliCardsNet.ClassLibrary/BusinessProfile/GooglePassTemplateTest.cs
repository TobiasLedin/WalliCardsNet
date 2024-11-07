﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalliCardsNet.ClassLibrary.BusinessProfile
{
    // TODO: Obsolete. Renamned to not mess with working API model
    // Support class PassMessage might be worth keeping though?

    public class GooglePassTemplateTest
    {
        public string ObjectId { get; set; } // Remove from DTO?
        public string ClassId { get; set; } // Remove from DTO?
        public string State { get; set; } // "ACTIVE", "INACTIVE"...
        public string CardTitle { get; set; }
        public string Header { get; set; }
        public string LogoImageUri { get; set; }
        public string WideLogoImageUri { get; set; }    // Replaces logo when used
        public string HexBackgroundColor { get; set; }
        public string HeroImageUri { get; set; }
        public List<TextModule> TextModulesData { get; set; } = [];
        public List<LinksModule> LinksModuleData { get; set; } = [];
        public List<ImageModule> ImageNodulesData { get; set; } = [];
        public List<PassMessage> Messages { get; set; } = [];    // Max 10 fields!
        public string BusinessId { get; set; }

        // Generic Class related
        public string FieldsJson { get; set; }
    }

   

    #region Support classes
    public class TextModule
    {
        public string Header { get; set; }
        public string Body { get; set; }
        public string Id { get; set; }
    }

    public class LinksModule
    {
        public string Uri { get; set; }
        public string Description { get; set; }
        public string Id { get; set; }
    }

    public class ImageModule
    {
        public string Uri { get; set; }
        public string Description { get; set; }
        public string Id { get; set; }
        public string Language { get; set; }
    }
    public class PassMessage
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Header { get; set; } // Message header
        public string Body { get; set; } // Message body
        public DateTime Start { get; set; } // Start time (UTC))
        public DateTime End { get; set; }   // End time (UTC))
        public string MessageType { get; set; } // Render as text on card detail or also as Android notification (enums "TEXT" or "TEXT_AND_NOTIFY")
    }

    #endregion
}