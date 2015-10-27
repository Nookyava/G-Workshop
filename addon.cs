using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WorkshopPub
{
    class Addon
    {
        public string addonname, addontype, addonfolderpath;
        public string[] addontags, addonignore;
        public string imagepath;
        private string gmadpath = Environment.CurrentDirectory + "/gmad";
        private string outputpath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/[G]Workshop/";
        private string outputname = "";

        public Addon() {
            addonname = "";
            addontype = "";
            addonfolderpath = "";

            string[] addontags = {};
            string[] addonignore = {};

            imagepath = "";
        }

        private void createErrorDialog(string message)
        {
            System.Windows.Forms.MessageBox.Show(message, "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
        }

        public void setJsonDetails(string setaddonname, string setaddontype, string[] setaddontags, string[] setaddonignores, string setaddonpath) {
            addonname = setaddonname;
            addontype = setaddontype;
            addontags = setaddontags;
            addonignore = setaddonignores;
            addonfolderpath = setaddonpath;

            outputname = addonname.Trim().ToLower();
        }

        public bool setImagePath(string setimagepath)
        {
            if (File.Exists(setimagepath))
            {
                System.Drawing.Image img = System.Drawing.Image.FromFile(setimagepath);
                int width = img.Width;
                int height = img.Height;

                if (width != 512 && height != 512)
                {
                    createErrorDialog("Uploaded image is not 512x512!");
                    return false;
                }

                imagepath = setimagepath;
                return true;
            }
            else
            {
                createErrorDialog("Issue with image chosen");
                return false;
            }
        }

        /*
         * Name: createJsonFile
         * Arguments: nil
         * Returns: nil
         * Desc: Creates the Json file with the input values necessary for us to create the .gma.
        */
        public bool createJsonFile()
        {
            if (addonfolderpath == null)
            {
                createErrorDialog("Folder path was not valid or did not exist.");
                return false;
            }

            if (String.IsNullOrWhiteSpace(addonname))
            {
                createErrorDialog("Json could not be created due to a blank addon name.");
                return false;
            }

            Console.WriteLine(addontype);

            if (String.IsNullOrWhiteSpace(addontype))
            {
                createErrorDialog("Json could not be created due to lack of a tag.");
                return false;
            }

            string path = Path.Combine(addonfolderpath, "addon.json");

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            var jsonformat = new {
                title = addonname,
                type = addontype,
                tags = addontags,
                ignore = addonignore
            };

            string jsonconverted = JsonConvert.SerializeObject(jsonformat, Newtonsoft.Json.Formatting.Indented);

            try {
                System.IO.File.WriteAllText(path, jsonconverted);
                return true;
            }
            catch (UnauthorizedAccessException e)
            {
                createErrorDialog(e.Message);
                return false;
            }
        }

        /*
         * Name: createGmaFile
         * Arguments: nil
         * Returns: nil
         * Desc: Packges the addon together with the .json we created so we can get ready to publish to workshop
        */
        public bool createGmaFile()
        {
            string docpath = Path.Combine(outputpath, outputname);

            if (!Directory.Exists(docpath))
            {
                System.IO.Directory.CreateDirectory(docpath);
            }

            if (!File.Exists(Path.Combine(addonfolderpath, "addon.json"))) {
                createErrorDialog("There doesn't appear to be a .json file, stopping creation of the .gma");
                return false;
            }

            string gmapath = Path.Combine(docpath, outputname);
            string filename = Path.Combine(gmadpath, "gmad.exe");
            string gmacmd = "create -folder \"" + addonfolderpath + "\" -out \"" + gmapath + ".gma\"";

            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = filename;
            p.StartInfo.Arguments = gmacmd;
            p.EnableRaisingEvents = true;
            p.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;

            p.Exited += (S, E) =>
            {
                System.Diagnostics.Debug.WriteLine(p.ExitCode);
            };

            p.Start();

            return true;
        }

        /*
         * Name: uploadToWorkshop
         * Arguments: nil
         * Returns: nil
         * Desc: Uploads the finalized .gma to the workshop
        */
        public void uploadToWorkshop()
        {
            // Major thanks to cartman300 (https://facepunch.com/member.php?u=404870) for the help below.
            string outputline = Path.Combine(outputpath, outputname + "/" + outputname + ".gma");
            string filename = Path.Combine(gmadpath, "gmpublish.exe");
            string gmacmd = "create -addon \"" + outputline + "\" -icon \"" + imagepath + "\"";

            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = filename;
            p.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            p.StartInfo.Arguments = gmacmd;
            p.EnableRaisingEvents = true;

            p.Exited += (S, E) =>
            {
                int code = p.ExitCode;

                if (code == 0)
                {
                    System.Windows.Forms.DialogResult result = System.Windows.Forms.MessageBox.Show("Success!", "Addon published successfully!", System.Windows.Forms.MessageBoxButtons.OK, 
                        System.Windows.Forms.MessageBoxIcon.Information);
                }
            };

            p.Start();
        }
    }
}
