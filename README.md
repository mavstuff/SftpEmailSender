## SftpEmailSender

**SftpEmailSender** is an app that downloads files from SFTP server and sends them to email

This is a C# .NET 6.0. application that downloads new files from an SFTP server, compresses them into a single ZIP file, and sends the ZIP file via email using System.Net.Mail and System.IO.Compression:

Replace the placeholder values for SFTP and email settings with your own credentials:

**SFTP server credentials and path**
string sftpHost = "your_sftp_host";
int sftpPort = 22; // Default SFTP port
string sftpUser = "your_sftp_login";
string sftpPass = "your_sftp_password";
string sftpRemotePath = "/";


**Email credentials and settings**
string smtpHost = "your_smtp_server";
int smtpPort = 587; // Default SMTP port
string smtpUser = "your_email_address";
string smtpPass = "your_email_password";
string fromEmail = "your_email_address";
string[] toEmails = new string[] { "to_email@address_1", "to_email@address_2" };


**Path to sent files tracking file**
string sentFilesPath = "_sent.txt";

**Path to cache files**
string localFolderPath = "C:\\sftp-cache";

**You can also modify Email subject**
string emailSubject = string.Format("Files {0:dd-MMM-yyyy}", DateTime.Now);

To run the application on a schedule, you can use the Windows Task Scheduler or another scheduling tool to execute the compiled application at specific intervals.

(c) 2023 Artem Moroz, artem.moroz@gmail.com
