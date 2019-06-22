using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi
{
    public class ContentTypes
    {
        #region types
        public static IHeaderItem TEXT = new HeaderItem("Content-Type: text/plain\r\n");
        public static IHeaderItem TEXT_UTF8 = new HeaderItem("Content-Type: text/plain; charset=UTF-8\r\n");
        public static IHeaderItem ZIP = new HeaderItem("Content-Type: application/zip\r\n");
        public static IHeaderItem OCTET_STREAM = new HeaderItem("Content-Type: application/octet-stream\r\n");
        public static IHeaderItem PDF = new HeaderItem("Content-Type: application/pdf\r\n");
        public static IHeaderItem POSTSCRIPT = new HeaderItem("Content-Type: application/postscript\r\n");
        public static IHeaderItem ATOM_XML = new HeaderItem("Content-Type: application/atom+xml\r\n");
        public static IHeaderItem ECMASCRIPT = new HeaderItem("Content-Type: application/ecmascript\r\n");
        public static IHeaderItem EDI_X12 = new HeaderItem("Content-Type: application/EDI-X12\r\n");
        public static IHeaderItem EDIFACT = new HeaderItem("Content-Type: application/EDIFACT\r\n");
        public static IHeaderItem JSON = new HeaderItem("Content-Type: application/json\r\n");
        public static IHeaderItem JAVASCRIPT = new HeaderItem("Content-Type: application/javascript\r\n");
        public static IHeaderItem OGG = new HeaderItem("Content-Type: application/ogg\r\n");
        public static IHeaderItem RDF_XML = new HeaderItem("Content-Type: application/rdf+xml\r\n");
        public static IHeaderItem RSS_XML = new HeaderItem("Content-Type: application/rss+xml\r\n");
        public static IHeaderItem SOAP_XML = new HeaderItem("Content-Type: application/soap+xml\r\n");
        public static IHeaderItem FONT_WOFF = new HeaderItem("Content-Type: application/font-woff\r\n");
        public static IHeaderItem XHTML_XML = new HeaderItem("Content-Type: application/xhtml+xml\r\n");
        public static IHeaderItem XML = new HeaderItem("Content-Type: text/xml\r\n");
        public static IHeaderItem XML_DTD = new HeaderItem("Content-Type: application/xml-dtd\r\n");
        public static IHeaderItem XOP_XML = new HeaderItem("Content-Type: application/xop+xml\r\n");
        public static IHeaderItem GZIP = new HeaderItem("Content-Type: application/gzip\r\n");
        public static IHeaderItem X_XLS = new HeaderItem("Content-Type: application/x-xls\r\n");
        public static IHeaderItem X_001 = new HeaderItem("Content-Type: application/x-001\r\n");
        public static IHeaderItem X_301 = new HeaderItem("Content-Type: application/x-301\r\n");
        public static IHeaderItem X_906 = new HeaderItem("Content-Type: application/x-906\r\n");
        public static IHeaderItem X_A11 = new HeaderItem("Content-Type: application/x-a11\r\n");
        public static IHeaderItem VND_ADOBE_WORKFLOW = new HeaderItem("Content-Type: application/vnd.adobe.workflow\r\n");
        public static IHeaderItem X_BMP = new HeaderItem("Content-Type: application/x-bmp\r\n");
        public static IHeaderItem X_C4T = new HeaderItem("Content-Type: application/x-c4t\r\n");
        public static IHeaderItem X_CALS = new HeaderItem("Content-Type: application/x-cals\r\n");
        public static IHeaderItem X_NETCDF = new HeaderItem("Content-Type: application/x-netcdf\r\n");
        public static IHeaderItem X_CEL = new HeaderItem("Content-Type: application/x-cel\r\n");
        public static IHeaderItem X_G4 = new HeaderItem("Content-Type: application/x-g4\r\n");
        public static IHeaderItem X_CIT = new HeaderItem("Content-Type: application/x-cit\r\n");
        public static IHeaderItem X_BOT = new HeaderItem("Content-Type: application/x-bot\r\n");
        public static IHeaderItem X_C90 = new HeaderItem("Content-Type: application/x-c90\r\n");
        public static IHeaderItem VND_MS_PKI_SECCAT = new HeaderItem("Content-Type: application/vnd.ms-pki.seccat\r\n");
        public static IHeaderItem X_CDR = new HeaderItem("Content-Type: application/x-cdr\r\n");
        public static IHeaderItem X_X509_CA_CERT = new HeaderItem("Content-Type: application/x-x509-ca-cert\r\n");
        public static IHeaderItem X_CGM = new HeaderItem("Content-Type: application/x-cgm\r\n");
        public static IHeaderItem X_CMX = new HeaderItem("Content-Type: application/x-cmx\r\n");
        public static IHeaderItem PKIX_CRL = new HeaderItem("Content-Type: application/pkix-crl\r\n");
        public static IHeaderItem X_CSI = new HeaderItem("Content-Type: application/x-csi\r\n");
        public static IHeaderItem X_CUT = new HeaderItem("Content-Type: application/x-cut\r\n");
        public static IHeaderItem X_DBM = new HeaderItem("Content-Type: application/x-dbm\r\n");
        public static IHeaderItem X_CMP = new HeaderItem("Content-Type: application/x-cmp\r\n");
        public static IHeaderItem X_COT = new HeaderItem("Content-Type: application/x-cot\r\n");
        public static IHeaderItem X_DBF = new HeaderItem("Content-Type: application/x-dbf\r\n");
        public static IHeaderItem X_DBX = new HeaderItem("Content-Type: application/x-dbx\r\n");
        public static IHeaderItem X_DCX = new HeaderItem("Content-Type: application/x-dcx\r\n");
        public static IHeaderItem X_DGN = new HeaderItem("Content-Type: application/x-dgn\r\n");
        public static IHeaderItem X_MSDOWNLOAD = new HeaderItem("Content-Type: application/x-msdownload\r\n");
        public static IHeaderItem MSWORD = new HeaderItem("Content-Type: application/msword\r\n");
        public static IHeaderItem X_DIB = new HeaderItem("Content-Type: application/x-dib\r\n");
        public static IHeaderItem X_DRW = new HeaderItem("Content-Type: application/x-drw\r\n");
        public static IHeaderItem X_DWF = new HeaderItem("Content-Type: application/x-dwf\r\n");
        public static IHeaderItem X_DXB = new HeaderItem("Content-Type: application/x-dxb\r\n");
        public static IHeaderItem VND_ADOBE_EDN = new HeaderItem("Content-Type: application/vnd.adobe.edn\r\n");
        public static IHeaderItem X_DWG = new HeaderItem("Content-Type: application/x-dwg\r\n");
        public static IHeaderItem X_DXF = new HeaderItem("Content-Type: application/x-dxf\r\n");
        public static IHeaderItem X_EMF = new HeaderItem("Content-Type: application/x-emf\r\n");
        public static IHeaderItem X_EPI = new HeaderItem("Content-Type: application/x-epi\r\n");
        public static IHeaderItem VND_FDF = new HeaderItem("Content-Type: application/vnd.fdf\r\n");
        public static IHeaderItem X_PS = new HeaderItem("Content-Type: application/x-ps\r\n");
        public static IHeaderItem X_EBX = new HeaderItem("Content-Type: application/x-ebx\r\n");
        public static IHeaderItem FRACTALS = new HeaderItem("Content-Type: application/fractals\r\n");
        public static IHeaderItem X_FRM = new HeaderItem("Content-Type: application/x-frm\r\n");
        public static IHeaderItem X_GBR = new HeaderItem("Content-Type: application/x-gbr\r\n");
        public static IHeaderItem X_GL2 = new HeaderItem("Content-Type: application/x-gl2\r\n");
        public static IHeaderItem X_HGL = new HeaderItem("Content-Type: application/x-hgl\r\n");
        public static IHeaderItem X_HPGL = new HeaderItem("Content-Type: application/x-hpgl\r\n");
        public static IHeaderItem MAC_BINHEX40 = new HeaderItem("Content-Type: application/mac-binhex40\r\n");
        public static IHeaderItem HTA = new HeaderItem("Content-Type: application/hta\r\n");
        public static IHeaderItem X_GP4 = new HeaderItem("Content-Type: application/x-gp4\r\n");
        public static IHeaderItem X_HMR = new HeaderItem("Content-Type: application/x-hmr\r\n");
        public static IHeaderItem X_HPL = new HeaderItem("Content-Type: application/x-hpl\r\n");
        public static IHeaderItem X_HRF = new HeaderItem("Content-Type: application/x-hrf\r\n");
        public static IHeaderItem X_ICB = new HeaderItem("Content-Type: application/x-icb\r\n");
        public static IHeaderItem X_ICO = new HeaderItem("Content-Type: application/x-ico\r\n");
        public static IHeaderItem X_IPHONE = new HeaderItem("Content-Type: application/x-iphone\r\n");
        public static IHeaderItem X_INTERNET_SIGNUP = new HeaderItem("Content-Type: application/x-internet-signup\r\n");
        public static IHeaderItem X_IFF = new HeaderItem("Content-Type: application/x-iff\r\n");
        public static IHeaderItem X_IGS = new HeaderItem("Content-Type: application/x-igs\r\n");
        public static IHeaderItem X_IMG = new HeaderItem("Content-Type: application/x-img\r\n");
        public static IHeaderItem X_JPE = new HeaderItem("Content-Type: application/x-jpe\r\n");
        public static IHeaderItem X_JAVASCRIPT = new HeaderItem("Content-Type: application/x-javascript\r\n");
        public static IHeaderItem X_JPG = new HeaderItem("Content-Type: application/x-jpg\r\n");
        public static IHeaderItem X_LAPLAYER_REG = new HeaderItem("Content-Type: application/x-laplayer-reg\r\n");
        public static IHeaderItem X_LATEX = new HeaderItem("Content-Type: application/x-latex\r\n");
        public static IHeaderItem X_LBM = new HeaderItem("Content-Type: application/x-lbm\r\n");
        public static IHeaderItem X_LTR = new HeaderItem("Content-Type: application/x-ltr\r\n");
        public static IHeaderItem X_TROFF_MAN = new HeaderItem("Content-Type: application/x-troff-man\r\n");
        public static IHeaderItem MSACCESS = new HeaderItem("Content-Type: application/msaccess\r\n");
        public static IHeaderItem X_MAC = new HeaderItem("Content-Type: application/x-mac\r\n");
        public static IHeaderItem X_MDB = new HeaderItem("Content-Type: application/x-mdb\r\n");
        public static IHeaderItem X_SHOCKWAVE_FLASH = new HeaderItem("Content-Type: application/x-shockwave-flash\r\n");
        public static IHeaderItem X_MI = new HeaderItem("Content-Type: application/x-mi\r\n");
        public static IHeaderItem X_MIL = new HeaderItem("Content-Type: application/x-mil\r\n");
        public static IHeaderItem VND_MS_PROJECT = new HeaderItem("Content-Type: application/vnd.ms-project\r\n");
        public static IHeaderItem X_MMXP = new HeaderItem("Content-Type: application/x-mmxp\r\n");
        public static IHeaderItem X_NRF = new HeaderItem("Content-Type: application/x-nrf\r\n");
        public static IHeaderItem X_OUT = new HeaderItem("Content-Type: application/x-out\r\n");
        public static IHeaderItem X_PKCS12 = new HeaderItem("Content-Type: application/x-pkcs12\r\n");
        public static IHeaderItem PKCS7_MIME = new HeaderItem("Content-Type: application/pkcs7-mime\r\n");
        public static IHeaderItem X_PKCS7_CERTREQRESP = new HeaderItem("Content-Type: application/x-pkcs7-certreqresp\r\n");
        public static IHeaderItem X_PC5 = new HeaderItem("Content-Type: application/x-pc5\r\n");
        public static IHeaderItem X_PCL = new HeaderItem("Content-Type: application/x-pcl\r\n");
        public static IHeaderItem VND_ADOBE_PDX = new HeaderItem("Content-Type: application/vnd.adobe.pdx\r\n");
        public static IHeaderItem X_PGL = new HeaderItem("Content-Type: application/x-pgl\r\n");
        public static IHeaderItem VND_MS_PKI_PKO = new HeaderItem("Content-Type: application/vnd.ms-pki.pko\r\n");
        public static IHeaderItem PKCS10 = new HeaderItem("Content-Type: application/pkcs10\r\n");
        public static IHeaderItem X_PKCS7_CERTIFICATES = new HeaderItem("Content-Type: application/x-pkcs7-certificates\r\n");
        public static IHeaderItem PKCS7_SIGNATURE = new HeaderItem("Content-Type: application/pkcs7-signature\r\n");
        public static IHeaderItem X_PCI = new HeaderItem("Content-Type: application/x-pci\r\n");
        public static IHeaderItem X_PCX = new HeaderItem("Content-Type: application/x-pcx\r\n");
        public static IHeaderItem X_PIC = new HeaderItem("Content-Type: application/x-pic\r\n");
        public static IHeaderItem X_PERL = new HeaderItem("Content-Type: application/x-perl\r\n");
        public static IHeaderItem X_PLT = new HeaderItem("Content-Type: application/x-plt\r\n");
        public static IHeaderItem X_PNG = new HeaderItem("Content-Type: application/x-png\r\n");
        public static IHeaderItem VND_MS_POWERPOINT = new HeaderItem("Content-Type: application/vnd.ms-powerpoint\r\n");
        public static IHeaderItem X_PPT = new HeaderItem("Content-Type: application/x-ppt\r\n");
        public static IHeaderItem PICS_RULES = new HeaderItem("Content-Type: application/pics-rules\r\n");
        public static IHeaderItem X_PRT = new HeaderItem("Content-Type: application/x-prt\r\n");
        public static IHeaderItem VND_RN_REALAUDIO = new HeaderItem("Content-Type: audio/vnd.rn-realaudio\r\n");
        public static IHeaderItem X_RAS = new HeaderItem("Content-Type: application/x-ras\r\n");
        public static IHeaderItem X_PPM = new HeaderItem("Content-Type: application/x-ppm\r\n");
        public static IHeaderItem X_PR = new HeaderItem("Content-Type: application/x-pr\r\n");
        public static IHeaderItem X_PRN = new HeaderItem("Content-Type: application/x-prn\r\n");
        public static IHeaderItem X_PTN = new HeaderItem("Content-Type: application/x-ptn\r\n");
        public static IHeaderItem X_RED = new HeaderItem("Content-Type: application/x-red\r\n");
        public static IHeaderItem VND_RN_REALSYSTEM_RJS = new HeaderItem("Content-Type: application/vnd.rn-realsystem-rjs\r\n");
        public static IHeaderItem X_RLC = new HeaderItem("Content-Type: application/x-rlc\r\n");
        public static IHeaderItem VND_RN_REALMEDIA = new HeaderItem("Content-Type: application/vnd.rn-realmedia\r\n");
        public static IHeaderItem RAT_FILE = new HeaderItem("Content-Type: application/rat-file\r\n");
        public static IHeaderItem VND_RN_RECORDING = new HeaderItem("Content-Type: application/vnd.rn-recording\r\n");
        public static IHeaderItem X_RGB = new HeaderItem("Content-Type: application/x-rgb\r\n");
        public static IHeaderItem VND_RN_REALSYSTEM_RJT = new HeaderItem("Content-Type: application/vnd.rn-realsystem-rjt\r\n");
        public static IHeaderItem X_RLE = new HeaderItem("Content-Type: application/x-rle\r\n");
        public static IHeaderItem VND_ADOBE_RMF = new HeaderItem("Content-Type: application/vnd.adobe.rmf\r\n");
        public static IHeaderItem VND_RN_REALSYSTEM_RMJ = new HeaderItem("Content-Type: application/vnd.rn-realsystem-rmj\r\n");
        public static IHeaderItem VND_RN_RN_MUSIC_PACKAGE = new HeaderItem("Content-Type: application/vnd.rn-rn_music_package\r\n");
        public static IHeaderItem VND_RN_REALMEDIA_VBR = new HeaderItem("Content-Type: application/vnd.rn-realmedia-vbr\r\n");
        public static IHeaderItem VND_RN_REALPLAYER = new HeaderItem("Content-Type: application/vnd.rn-realplayer\r\n");
        public static IHeaderItem X_PN_REALAUDIO_PLUGIN = new HeaderItem("Content-Type: audio/x-pn-realaudio-plugin\r\n");
        public static IHeaderItem VND_RN_REALMEDIA_SECURE = new HeaderItem("Content-Type: application/vnd.rn-realmedia-secure\r\n");
        public static IHeaderItem VND_RN_REALSYSTEM_RMX = new HeaderItem("Content-Type: application/vnd.rn-realsystem-rmx\r\n");
        public static IHeaderItem VND_RN_RSML = new HeaderItem("Content-Type: application/vnd.rn-rsml\r\n");
        public static IHeaderItem VND_RN_REALVIDEO = new HeaderItem("Content-Type: video/vnd.rn-realvideo\r\n");
        public static IHeaderItem X_SAT = new HeaderItem("Content-Type: application/x-sat\r\n");
        public static IHeaderItem X_SDW = new HeaderItem("Content-Type: application/x-sdw\r\n");
        public static IHeaderItem X_SLB = new HeaderItem("Content-Type: application/x-slb\r\n");
        public static IHeaderItem X_RTF = new HeaderItem("Content-Type: application/x-rtf\r\n");
        public static IHeaderItem X_SAM = new HeaderItem("Content-Type: application/x-sam\r\n");
        public static IHeaderItem SDP = new HeaderItem("Content-Type: application/sdp\r\n");
        public static IHeaderItem X_STUFFIT = new HeaderItem("Content-Type: application/x-stuffit\r\n");
        public static IHeaderItem X_SLD = new HeaderItem("Content-Type: application/x-sld\r\n");
        public static IHeaderItem SMIL = new HeaderItem("Content-Type: application/smil\r\n");
        public static IHeaderItem X_SMK = new HeaderItem("Content-Type: application/x-smk\r\n");
        public static IHeaderItem FUTURESPLASH = new HeaderItem("Content-Type: application/futuresplash\r\n");
        public static IHeaderItem STREAMINGMEDIA = new HeaderItem("Content-Type: application/streamingmedia\r\n");
        public static IHeaderItem VND_MS_PKI_STL = new HeaderItem("Content-Type: application/vnd.ms-pki.stl\r\n");
        public static IHeaderItem VND_MS_PKI_CERTSTORE = new HeaderItem("Content-Type: application/vnd.ms-pki.certstore\r\n");
        public static IHeaderItem X_TDF = new HeaderItem("Content-Type: application/x-tdf\r\n");
        public static IHeaderItem X_TGA = new HeaderItem("Content-Type: application/x-tga\r\n");
        public static IHeaderItem X_STY = new HeaderItem("Content-Type: application/x-sty\r\n");
        public static IHeaderItem X_TG4 = new HeaderItem("Content-Type: application/x-tg4\r\n");
        public static IHeaderItem X_TIF = new HeaderItem("Content-Type: application/x-tif\r\n");
        public static IHeaderItem VND_VISIO = new HeaderItem("Content-Type: application/vnd.visio\r\n");
        public static IHeaderItem X_VPEG005 = new HeaderItem("Content-Type: application/x-vpeg005\r\n");
        public static IHeaderItem X_VSD = new HeaderItem("Content-Type: application/x-vsd\r\n");
        public static IHeaderItem X_BITTORRENT = new HeaderItem("Content-Type: application/x-bittorrent\r\n");
        public static IHeaderItem X_VDA = new HeaderItem("Content-Type: application/x-vda\r\n");
        public static IHeaderItem X_VST = new HeaderItem("Content-Type: application/x-vst\r\n");
        public static IHeaderItem X_WB1 = new HeaderItem("Content-Type: application/x-wb1\r\n");
        public static IHeaderItem X_WB3 = new HeaderItem("Content-Type: application/x-wb3\r\n");
        public static IHeaderItem X_WK4 = new HeaderItem("Content-Type: application/x-wk4\r\n");
        public static IHeaderItem X_WKS = new HeaderItem("Content-Type: application/x-wks\r\n");
        public static IHeaderItem X_WB2 = new HeaderItem("Content-Type: application/x-wb2\r\n");
        public static IHeaderItem X_WK3 = new HeaderItem("Content-Type: application/x-wk3\r\n");
        public static IHeaderItem X_WKQ = new HeaderItem("Content-Type: application/x-wkq\r\n");
        public static IHeaderItem X_WMF = new HeaderItem("Content-Type: application/x-wmf\r\n");
        public static IHeaderItem X_MS_WMD = new HeaderItem("Content-Type: application/x-ms-wmd\r\n");
        public static IHeaderItem X_WP6 = new HeaderItem("Content-Type: application/x-wp6\r\n");
        public static IHeaderItem X_WPG = new HeaderItem("Content-Type: application/x-wpg\r\n");
        public static IHeaderItem X_WQ1 = new HeaderItem("Content-Type: application/x-wq1\r\n");
        public static IHeaderItem X_WRI = new HeaderItem("Content-Type: application/x-wri\r\n");
        public static IHeaderItem X_WS = new HeaderItem("Content-Type: application/x-ws\r\n");
        public static IHeaderItem X_MS_WMZ = new HeaderItem("Content-Type: application/x-ms-wmz\r\n");
        public static IHeaderItem X_WPD = new HeaderItem("Content-Type: application/x-wpd\r\n");
        public static IHeaderItem VND_MS_WPL = new HeaderItem("Content-Type: application/vnd.ms-wpl\r\n");
        public static IHeaderItem X_WR1 = new HeaderItem("Content-Type: application/x-wr1\r\n");
        public static IHeaderItem X_WRK = new HeaderItem("Content-Type: application/x-wrk\r\n");
        public static IHeaderItem VND_ADOBE_XDP = new HeaderItem("Content-Type: application/vnd.adobe.xdp\r\n");
        public static IHeaderItem VND_ADOBE_XFD = new HeaderItem("Content-Type: application/vnd.adobe.xfd\r\n");
        public static IHeaderItem VND_ADOBE_XFDF = new HeaderItem("Content-Type: application/vnd.adobe.xfdf\r\n");
        public static IHeaderItem VND_MS_EXCEL = new HeaderItem("Content-Type: application/vnd.ms-excel\r\n");
        public static IHeaderItem X_XWD = new HeaderItem("Content-Type: application/x-xwd\r\n");
        public static IHeaderItem VND_SYMBIAN_INSTALL = new HeaderItem("Content-Type: application/vnd.symbian.install\r\n");
        public static IHeaderItem X_X_T = new HeaderItem("Content-Type: application/x-x_t\r\n");
        public static IHeaderItem VND_ANDROID_PACKAGE_ARCHIVE = new HeaderItem("Content-Type: application/vnd.android.package-archive\r\n");
        public static IHeaderItem X_X_B = new HeaderItem("Content-Type: application/x-x_b\r\n");
        public static IHeaderItem VND_IPHONE = new HeaderItem("Content-Type: application/vnd.iphone\r\n");
        public static IHeaderItem X_SILVERLIGHT_APP = new HeaderItem("Content-Type: application/x-silverlight-app\r\n");
        public static IHeaderItem X_XLW = new HeaderItem("Content-Type: application/x-xlw\r\n");
        public static IHeaderItem SCPLS = new HeaderItem("Content-Type: audio/scpls\r\n");
        public static IHeaderItem X_ANV = new HeaderItem("Content-Type: application/x-anv\r\n");
        public static IHeaderItem X_ICQ = new HeaderItem("Content-Type: application/x-icq\r\n");
        public static IHeaderItem H323 = new HeaderItem("Content-Type: text/h323\r\n");
        public static IHeaderItem ASA = new HeaderItem("Content-Type: text/asa\r\n");
        public static IHeaderItem ASP = new HeaderItem("Content-Type: text/asp\r\n");
        public static IHeaderItem CSS = new HeaderItem("Content-Type: text/css\r\n");
        public static IHeaderItem CSV = new HeaderItem("Content-Type: text/csv\r\n");
        public static IHeaderItem X_COMPONENT = new HeaderItem("Content-Type: text/x-component\r\n");
        public static IHeaderItem HTML = new HeaderItem("Content-Type: text/html\r\n");
        public static IHeaderItem WEBVIEWHTML = new HeaderItem("Content-Type: text/webviewhtml\r\n");
        public static IHeaderItem VND_RN_REALTEXT = new HeaderItem("Content-Type: text/vnd.rn-realtext\r\n");
        public static IHeaderItem PLAIN = new HeaderItem("Content-Type: text/plain\r\n");
        public static IHeaderItem IULS = new HeaderItem("Content-Type: text/iuls\r\n");
        public static IHeaderItem X_VCARD = new HeaderItem("Content-Type: text/x-vcard\r\n");
        public static IHeaderItem VND_WAP_WML = new HeaderItem("Content-Type: text/vnd.wap.wml\r\n");
        public static IHeaderItem SCRIPTLET = new HeaderItem("Content-Type: text/scriptlet\r\n");
        public static IHeaderItem X_MS_ODC = new HeaderItem("Content-Type: text/x-ms-odc\r\n");
        public static IHeaderItem VND_RN_REALTEXT3D = new HeaderItem("Content-Type: text/vnd.rn-realtext3d\r\n");
        public static IHeaderItem X_MEI_AAC = new HeaderItem("Content-Type: audio/x-mei-aac\r\n");
        public static IHeaderItem AIFF = new HeaderItem("Content-Type: audio/aiff\r\n");
        public static IHeaderItem BASIC = new HeaderItem("Content-Type: audio/basic\r\n");
        public static IHeaderItem X_LIQUID_FILE = new HeaderItem("Content-Type: audio/x-liquid-file\r\n");
        public static IHeaderItem X_LIQUID_SECURE = new HeaderItem("Content-Type: audio/x-liquid-secure\r\n");
        public static IHeaderItem X_LA_LMS = new HeaderItem("Content-Type: audio/x-la-lms\r\n");
        public static IHeaderItem MPEGURL = new HeaderItem("Content-Type: audio/mpegurl\r\n");
        public static IHeaderItem MID = new HeaderItem("Content-Type: audio/mid\r\n");
        public static IHeaderItem MP2 = new HeaderItem("Content-Type: audio/mp2\r\n");
        public static IHeaderItem MP3 = new HeaderItem("Content-Type: audio/mp3\r\n");
        public static IHeaderItem MP4 = new HeaderItem("Content-Type: audio/mp4\r\n");
        public static IHeaderItem X_MUSICNET_DOWNLOAD = new HeaderItem("Content-Type: audio/x-musicnet-download\r\n");
        public static IHeaderItem MP1 = new HeaderItem("Content-Type: audio/mp1\r\n");
        public static IHeaderItem X_MUSICNET_STREAM = new HeaderItem("Content-Type: audio/x-musicnet-stream\r\n");
        public static IHeaderItem RN_MPEG = new HeaderItem("Content-Type: audio/rn-mpeg\r\n");
        public static IHeaderItem X_PN_REALAUDIO = new HeaderItem("Content-Type: audio/x-pn-realaudio\r\n");
        public static IHeaderItem WAV = new HeaderItem("Content-Type: audio/wav\r\n");
        public static IHeaderItem X_MS_WAX = new HeaderItem("Content-Type: audio/x-ms-wax\r\n");
        public static IHeaderItem X_MS_WMA = new HeaderItem("Content-Type: audio/x-ms-wma\r\n");
        public static IHeaderItem X_MS_ASF = new HeaderItem("Content-Type: video/x-ms-asf\r\n");
        public static IHeaderItem AVI = new HeaderItem("Content-Type: video/avi\r\n");
        public static IHeaderItem X_IVF = new HeaderItem("Content-Type: video/x-ivf\r\n");
        public static IHeaderItem X_MPEG = new HeaderItem("Content-Type: video/x-mpeg\r\n");
        public static IHeaderItem MPEG4 = new HeaderItem("Content-Type: video/mpeg4\r\n");
        public static IHeaderItem X_SGI_MOVIE = new HeaderItem("Content-Type: video/x-sgi-movie\r\n");
        public static IHeaderItem MPEG = new HeaderItem("Content-Type: video/mpeg\r\n");
        public static IHeaderItem X_MPG = new HeaderItem("Content-Type: video/x-mpg\r\n");
        public static IHeaderItem MPG = new HeaderItem("Content-Type: video/mpg\r\n");
        public static IHeaderItem X_MS_WM = new HeaderItem("Content-Type: video/x-ms-wm\r\n");
        public static IHeaderItem X_MS_WMV = new HeaderItem("Content-Type: video/x-ms-wmv\r\n");
        public static IHeaderItem X_MS_WMX = new HeaderItem("Content-Type: video/x-ms-wmx\r\n");
        public static IHeaderItem X_MS_WVX = new HeaderItem("Content-Type: video/x-ms-wvx\r\n");
        public static IHeaderItem TIFF = new HeaderItem("Content-Type: image/tiff\r\n");
        public static IHeaderItem FAX = new HeaderItem("Content-Type: image/fax\r\n");
        public static IHeaderItem GIF = new HeaderItem("Content-Type: image/gif\r\n");
        public static IHeaderItem X_ICON = new HeaderItem("Content-Type: image/x-icon\r\n");
        public static IHeaderItem JPEG = new HeaderItem("Content-Type: image/jpeg\r\n");
        public static IHeaderItem PNETVUE = new HeaderItem("Content-Type: image/pnetvue\r\n");
        public static IHeaderItem PNG = new HeaderItem("Content-Type: image/png\r\n");
        public static IHeaderItem VND_RN_REALPIX = new HeaderItem("Content-Type: image/vnd.rn-realpix\r\n");
        public static IHeaderItem VND_WAP_WBMP = new HeaderItem("Content-Type: image/vnd.wap.wbmp\r\n");
        public static IHeaderItem RFC822 = new HeaderItem("Content-Type: message/rfc822\r\n");
        public static IHeaderItem _907= new HeaderItem("Content-Type: drawing/907\r\n");
        public static IHeaderItem X_SLK = new HeaderItem("Content-Type: drawing/x-slk\r\n");
        public static IHeaderItem X_TOP = new HeaderItem("Content-Type: drawing/x-top\r\n");


        #endregion

        static ContentTypes()
        {
            System.IO.StringReader reader = new System.IO.StringReader(CONTENT_TYPES);
            string line = reader.ReadLine();
            while (line != null)
            {
                if (!string.IsNullOrEmpty(line))
                {
                    string[] values = line.Split(',');
                    if (values.Length == 2)
                    {
                        string ext = values[0].Replace(".", "");
                        mContent[ext] = values[1];
                    }
                }
                line = reader.ReadLine();
            }
        }

        private static Dictionary<string, string> mContent = new Dictionary<string, string>();

       private const string OTHER_OCTET_STREAM = "application/octet-stream";

        public static string GetContentType(string ext)
        {
            string value;
            if (!mContent.TryGetValue(ext, out value))
                value = OTHER_OCTET_STREAM;
            return value;
        }


        private const string CONTENT_TYPES = @"
.zip,application/zip
.rar,application/octet-stream
.pdf,application/pdf
.ai,application/postscript
.xml,application/atom+xml
.js,application/ecmascript
.edi,application/EDI-X12
.edi,application/EDIFACT
.json,application/json
.js,application/javascript
.ogg,application/ogg
.rdf,application/rdf+xml
.xml,application/rss+xml
.xml,application/soap+xml
.woff,application/font-woff
.xhtml,application/xhtml+xml
.xml,application/xml
.dtd,application/xml-dtd
.xml,application/xop+xml
.zip,application/zip
.gzip,application/gzip
.xls,application/x-xls
.001,application/x-001
.301,application/x-301
.906,application/x-906
.a11,application/x-a11
.awf,application/vnd.adobe.workflow
.bmp,application/x-bmp
.c4t,application/x-c4t
.cal,application/x-cals
.cdf,application/x-netcdf
.cel,application/x-cel
.cg4,application/x-g4
.cit,application/x-cit
.bot,application/x-bot
.c90,application/x-c90
.cat,application/vnd.ms-pki.seccat
.cdr,application/x-cdr
.cer,application/x-x509-ca-cert
.cgm,application/x-cgm
.cmx,application/x-cmx
.crl,application/pkix-crl
.csi,application/x-csi
.cut,application/x-cut
.dbm,application/x-dbm
.cmp,application/x-cmp
.cot,application/x-cot
.crt,application/x-x509-ca-cert
.dbf,application/x-dbf
.dbx,application/x-dbx
.dcx,application/x-dcx
.dgn,application/x-dgn
.dll,application/x-msdownload
.dot,application/msword
.der,application/x-x509-ca-cert
.dib,application/x-dib
.doc,application/msword
.drw,application/x-drw
.dwf,application/x-dwf
.dxb,application/x-dxb
.edn,application/vnd.adobe.edn
.dwg,application/x-dwg
.dxf,application/x-dxf
.emf,application/x-emf
.epi,application/x-epi
.eps,application/postscript
.exe,application/x-msdownload
.fdf,application/vnd.fdf
.eps,application/x-ps
.etd,application/x-ebx
.fif,application/fractals
.frm,application/x-frm
.gbr,application/x-gbr
.g4,application/x-g4
.gl2,application/x-gl2
.hgl,application/x-hgl
.hpg,application/x-hpgl
.hqx,application/mac-binhex40
.hta,application/hta
.gp4,application/x-gp4
.hmr,application/x-hmr
.hpl,application/x-hpl
.hrf,application/x-hrf
.icb,application/x-icb
.ico,application/x-ico
.ig4,application/x-g4
.iii,application/x-iphone
.ins,application/x-internet-signup
.iff,application/x-iff
.igs,application/x-igs
.img,application/x-img
.isp,application/x-internet-signup
.jpe,application/x-jpe
.js,application/x-javascript
.jpg,application/x-jpg
.lar,application/x-laplayer-reg
.latex,application/x-latex
.lbm,application/x-lbm
.ls,application/x-javascript
.ltr,application/x-ltr
.man,application/x-troff-man
.mdb,application/msaccess
.mac,application/x-mac
.mdb,application/x-mdb
.mfp,application/x-shockwave-flash
.mi,application/x-mi
.mil,application/x-mil
.mocha,application/x-javascript
.mpd,application/vnd.ms-project
.mpp,application/vnd.ms-project
.mpt,application/vnd.ms-project
.mpw,application/vnd.ms-project
.mpx,application/vnd.ms-project
.mxp,application/x-mmxp
.nrf,application/x-nrf
.out,application/x-out
.p12,application/x-pkcs12
.p7c,application/pkcs7-mime
.p7r,application/x-pkcs7-certreqresp
.pc5,application/x-pc5
.pcl,application/x-pcl
.pdx,application/vnd.adobe.pdx
.pgl,application/x-pgl
.pko,application/vnd.ms-pki.pko
.p10,application/pkcs10
.p7b,application/x-pkcs7-certificates
.p7m,application/pkcs7-mime
.p7s,application/pkcs7-signature
.pci,application/x-pci
.pcx,application/x-pcx
.pdf,application/pdf
.pfx,application/x-pkcs12
.pic,application/x-pic
.pl,application/x-perl
.plt,application/x-plt
.png,application/x-png
.ppa,application/vnd.ms-powerpoint
.pps,application/vnd.ms-powerpoint
.ppt,application/x-ppt
.prf,application/pics-rules
.prt,application/x-prt
.ps,application/postscript
.pwz,application/vnd.ms-powerpoint
.ra,audio/vnd.rn-realaudio
.ras,application/x-ras
.pot,application/vnd.ms-powerpoint
.ppm,application/x-ppm
.ppt,application/vnd.ms-powerpoint
.pr,application/x-pr
.prn,application/x-prn
.ps,application/x-ps
.ptn,application/x-ptn
.red,application/x-red
.rjs,application/vnd.rn-realsystem-rjs
.rlc,application/x-rlc
.rm,application/vnd.rn-realmedia
.rat,application/rat-file
.rec,application/vnd.rn-recording
.rgb,application/x-rgb
.rjt,application/vnd.rn-realsystem-rjt
.rle,application/x-rle
.rmf,application/vnd.adobe.rmf
.rmj,application/vnd.rn-realsystem-rmj
.rmp,application/vnd.rn-rn_music_package
.rmvb,application/vnd.rn-realmedia-vbr
.rnx,application/vnd.rn-realplayer
.rpm,audio/x-pn-realaudio-plugin
.rms,application/vnd.rn-realmedia-secure
.rmx,application/vnd.rn-realsystem-rmx
.rsml,application/vnd.rn-rsml
.rtf,application/msword
.rv,video/vnd.rn-realvideo
.sat,application/x-sat
.sdw,application/x-sdw
.slb,application/x-slb
.rtf,application/x-rtf
.sam,application/x-sam
.sdp,application/sdp
.sit,application/x-stuffit
.sld,application/x-sld
.smi,application/smil
.smk,application/x-smk
.smil,application/smil
.spc,application/x-pkcs7-certificates
.spl,application/futuresplash
.ssm,application/streamingmedia
.stl,application/vnd.ms-pki.stl
.sst,application/vnd.ms-pki.certstore
.tdf,application/x-tdf
.tga,application/x-tga
.sty,application/x-sty
.swf,application/x-shockwave-flash
.tg4,application/x-tg4
.tif,application/x-tif
.vdx,application/vnd.visio
.vpg,application/x-vpeg005
.vsd,application/x-vsd
.vst,application/vnd.visio
.vsw,application/vnd.visio
.vtx,application/vnd.visio
.torrent,application/x-bittorrent
.vda,application/x-vda
.vsd,application/vnd.visio
.vss,application/vnd.visio
.vst,application/x-vst
.vsx,application/vnd.visio
.wb1,application/x-wb1
.wb3,application/x-wb3
.wiz,application/msword
.wk4,application/x-wk4
.wks,application/x-wks
.wb2,application/x-wb2
.wk3,application/x-wk3
.wkq,application/x-wkq
.wmf,application/x-wmf
.wmd,application/x-ms-wmd
.wp6,application/x-wp6
.wpg,application/x-wpg
.wq1,application/x-wq1
.wri,application/x-wri
.ws,application/x-ws
.wmz,application/x-ms-wmz
.wpd,application/x-wpd
.wpl,application/vnd.ms-wpl
.wr1,application/x-wr1
.wrk,application/x-wrk
.ws2,application/x-ws
.xdp,application/vnd.adobe.xdp
.xfd,application/vnd.adobe.xfd
.xfdf,application/vnd.adobe.xfdf
.xls,application/vnd.ms-excel
.xwd,application/x-xwd
.sis,application/vnd.symbian.install
.x_t,application/x-x_t
.apk,application/vnd.android.package-archive
.x_b,application/x-x_b
.sisx,application/vnd.symbian.install
.ipa,application/vnd.iphone
.xap,application/x-silverlight-app
.xlw,application/x-xlw
.xpl,audio/scpls
.anv,application/x-anv
.uin,application/x-icq
.323,text/h323
.biz,text/xml
.cml,text/xml
.asa,text/asa
.asp,text/asp
.css,text/css
.csv,text/csv
.dcd,text/xml
.dtd,text/xml
.ent,text/xml
.fo,text/xml
.htc,text/x-component
.html,text/html
.htx,text/html
.htm,text/html
.htt,text/webviewhtml
.jsp,text/html
.math,text/xml
.mml,text/xml
.mtx,text/xml
.plg,text/html
.rdf,text/xml
.rt,text/vnd.rn-realtext
.sol,text/plain
.spp,text/xml
.stm,text/html
.svg,text/xml
.tld,text/xml
.txt,text/plain
.uls,text/iuls
.vml,text/xml
.tsd,text/xml
.vcf,text/x-vcard
.vxml,text/xml
.wml,text/vnd.wap.wml
.wsdl,text/xml
.wsc,text/scriptlet
.xdr,text/xml
.xql,text/xml
.xsd,text/xml
.xslt,text/xml
.xml,text/xml
.xq,text/xml
.xquery,text/xml
.xsl,text/xml
.xhtml,text/html
.odc,text/x-ms-odc
.r3t,text/vnd.rn-realtext3d
.sor,text/plain
.acp,audio/x-mei-aac
.aif,audio/aiff
.aiff,audio/aiff
.aifc,audio/aiff
.au,audio/basic
.la1,audio/x-liquid-file
.lavs,audio/x-liquid-secure
.lmsff,audio/x-la-lms
.m3u,audio/mpegurl
.midi,audio/mid
.mid,audio/mid
.mp2,audio/mp2
.mp3,audio/mp3
.mp4,audio/mp4
.mnd,audio/x-musicnet-download
.mp1,audio/mp1
.mns,audio/x-musicnet-stream
.mpga,audio/rn-mpeg
.pls,audio/scpls
.ram,audio/x-pn-realaudio
.rmi,audio/mid
.rmm,audio/x-pn-realaudio
.snd,audio/basic
.wav,audio/wav
.wax,audio/x-ms-wax
.wma,audio/x-ms-wma
.asf,video/x-ms-asf
.asx,video/x-ms-asf
.avi,video/avi
.IVF,video/x-ivf
.m1v,video/x-mpeg
.m2v,video/x-mpeg
.m4e,video/mpeg4
.movie,video/x-sgi-movie
.mp2v,video/mpeg
.mp4,video/mpeg4
.mpa,video/x-mpg
.mpe,video/x-mpeg
.mpg,video/mpg
.mpeg,video/mpg
.mps,video/x-mpeg
.mpv,video/mpg
.mpv2,video/mpeg
.wm,video/x-ms-wm
.wmv,video/x-ms-wmv
.wmx,video/x-ms-wmx
.wvx,video/x-ms-wvx
.tif,image/tiff
.fax,image/fax
.gif,image/gif
.ico,image/x-icon
.jfif,image/jpeg
.jpe,image/jpeg
.jpeg,image/jpeg
.jpg,image/jpeg
.net,image/pnetvue
.png,image/png
.rp,image/vnd.rn-realpix
.tif,image/tiff
.tiff,image/tiff
.wbmp,image/vnd.wap.wbmp
.eml,message/rfc822
.mht,message/rfc822
.mhtml,message/rfc822
.nws,message/rfc822
.907,drawing/907
.slk,drawing/x-slk
.top,drawing/x-top
";
    }
}
