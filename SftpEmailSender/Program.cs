/* 
 * SftpEmailSender is an app that downloads files from SFTP server and sends them to email
 * 
 * (c) 2023 Artem Moroz, artem.moroz@gmail.com
 */

using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Globalization;
using Renci.SshNet;

namespace SftpEmailSender
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // SFTP server credentials and path
            string sftpHost = "your_sftp_host";
            int sftpPort = 22; // Default SFTP port
            string sftpUser = "your_sftp_login";
            string sftpPass = "your_sfpt_password";
            string sftpRemotePath = "/";



            // Email credentials and settings
            string smtpHost = "your_smtp_server";
            int smtpPort = 587; // Default SMTP port
            string smtpUser = "your_email_address";
            string smtpPass = "your_email_password";
            string fromEmail = "your_email_address";
            string[] toEmails = new string[] { "to_email@address_1", "to_email@address_2" };


            // Path to sent files tracking file
            string sentFilesPath = "_sent.txt";
            //Path to cache files
            string localFolderPath = "C:\\sftp-cache";

            //Email subject
            string emailSubject = string.Format("Files {0:dd-MMM-yyyy}", DateTime.Now);

            // Load the list of sent files
            var sentFiles = File.Exists(sentFilesPath) ? File.ReadAllLines(sentFilesPath).ToList() : new List<string>();

            // Connect to the SFTP server and download new files
            using (var sftpClient = new SftpClient(sftpHost, sftpPort, sftpUser, sftpPass))
            {
                sftpClient.Connect();

                var remoteFiles = sftpClient.ListDirectory(sftpRemotePath).Where(f => !f.IsDirectory).ToList();

                if (remoteFiles.Count > 0)
                {
                    var unsentFiles = new List<string>();

                    foreach (var file in remoteFiles)
                    {
                        // Download file to local folder
                        string localFilePath = Path.Combine(localFolderPath, file.Name);
                        using (var fileStream = File.OpenWrite(localFilePath))
                        {
                            sftpClient.DownloadFile(file.FullName, fileStream);
                        }

                        // Check if the file has been sent before
                        if (!sentFiles.Contains(file.Name))
                        {
                            unsentFiles.Add(localFilePath);
                        }
                    }

                    if (unsentFiles.Count > 0)
                    {
                        // Create a new email
                        using (var message = new MailMessage())
                        {
                            message.From = new MailAddress(fromEmail);
                            foreach (string email in toEmails)
                                message.To.Add(email);

                            message.Subject = emailSubject;
                            message.SubjectEncoding = Encoding.UTF8;

                            // Compress the unsent files into a single ZIP file
                            using (var zipStream = new MemoryStream())
                            {
                                using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
                                {
                                    foreach (var localFilePath in unsentFiles)
                                    {
                                        using (var fileStream = File.OpenRead(localFilePath))
                                        {
                                            var entry = archive.CreateEntry(Path.GetFileName(localFilePath));
                                            using (var entryStream = entry.Open())
                                            {
                                                fileStream.CopyTo(entryStream);
                                            }
                                        }
                                    }
                                }

                                zipStream.Position = 0;

                                // Attach the ZIP file to the email
                                message.Attachments.Add(new Attachment(zipStream, $"{emailSubject}.zip", "application/zip"));

                                // Send the email
                                using (var smtpClient = new SmtpClient(smtpHost, smtpPort))
                                {
                                    smtpClient.EnableSsl = true;
                                    smtpClient.Credentials = new System.Net.NetworkCredential(smtpUser, smtpPass);
                                    smtpClient.Send(message);
                                }

                                Console.WriteLine("Email sent successfully with new files.");
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("No new files found on the SFTP server.");
                    }



                    // Update the sent files list and save it to "sent.txt"
                    foreach (var localFilePath in unsentFiles)
                    {
                        sentFiles.Add(Path.GetFileName(localFilePath));
                    }
                    File.WriteAllLines(sentFilesPath, sentFiles);
                }
                else
                {
                    Console.WriteLine("No new files found on the SFTP server.");
                }

                sftpClient.Disconnect();
            }

        }

    }
}