using log4net;
using log4net.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mcd.App.GetXmlRpc.Helpers
{
    public class logger
    {
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static string dateExecution = DateTime.Now.ToString("yyyyMMddHHmm");
        public logger()
        {
            log4net.Config.XmlConfigurator.Configure();
            GlobalContext.Properties["processLogId"] = dateExecution;
        }

        public void Debug(string message, int? restaurantId = null, TimeSpan? duree = null)
        {
            log4net.LogicalThreadContext.Properties["restaurantId"] = restaurantId != null ? restaurantId.ToString() : "";
            log4net.LogicalThreadContext.Properties["duree"] = duree != null ? duree.Value.TotalSeconds.ToString() : "";
            _logger.Debug(message);
        }

        public void Info(string message, int? restaurantId = null, TimeSpan? duree = null)
        {
            log4net.LogicalThreadContext.Properties["restaurantId"] = restaurantId != null ? restaurantId.ToString() : "";
            log4net.LogicalThreadContext.Properties["duree"] = duree != null ? duree.Value.TotalSeconds.ToString() : "";
            _logger.Info(message);
        }
        public void Warn(string message, int? restaurantId = null, TimeSpan? duree = null)
        {
            log4net.LogicalThreadContext.Properties["restaurantId"] = restaurantId != null ? restaurantId.ToString() : "";
            log4net.LogicalThreadContext.Properties["duree"] = duree != null ? duree.Value.TotalSeconds.ToString() : "";
            _logger.Warn(message);
        }
        public void Error(string message, Exception ex, int? restaurantId = null, TimeSpan? duree = null)
        {
            log4net.LogicalThreadContext.Properties["restaurantId"] = restaurantId != null ? restaurantId.ToString() : "";
            log4net.LogicalThreadContext.Properties["duree"] = duree != null ? duree.Value.TotalSeconds.ToString() : "";
            _logger.Error(message, ex);
        }

    }
}
