using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Bugtracker.Ticketing
{
    /// <summary>
    /// To be used as a base class for all ticket creation objects. Not used yet...
    /// </summary>
    public class TicketCreationObject
    {
        public string       TicketName          { get; set; }
        public string       TicketDescription   { get; set; }
        public string       ServerPath          { get; set; }
        public List<Bitmap> Screenshots         { get; set; }
        public List<string> LogFiles            { get; set; }
        public string       ComputerName        { get; set; }
        public string       Location            { get; set; }
        

        public TicketCreationObject(string ticketName, string ticketDescription, string serverPath, string computerName, string Location)
        {

        }

        public void AddScreenshot()
        {
            //this.LogFiles
        }

        public void AddLogFile()
        {

        }

        public virtual bool Create()
        {
            throw new NotImplementedException();
        }
    }
}