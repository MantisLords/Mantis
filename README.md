# Mantis
Library to process simple measurement data and directly insert them into LaTex-Files

## Getting started
To use Mantis you need an IDE. The strong recommendation is https://www.jetbrains.com/rider/.
Then you need to clone the repository. 

To work with the TeX-files you need the TeXiFy-Idea plugin for Rider.
Please follow the installation instructions https://github.com/Hannah-Sten/TeXiFy-IDEA/wiki/Installation#windows-instructions 
to get started.
(There are also equivalent plugins for vs code but you need to get them started your self)

This library works by you inserting your data in csv-files in the Mantis.Workspace solution.
Then you will import that data into your code where you can modify and process it. Afterwards
you can generate TeX-Files where you can output your data as tables and plots. Those generated
TeX-Files then can be referenced from your Main-TeX-File. Which in turn can be edited and
then complied via the TeXiFy-Plugin.

Examples coming soon