# N-Backer

*N-backer - Copyright ©2011-2012 School of Computing Science, Newcastle University*

**Paper:** Monk, A. F., Jackson, D., Nielsen, D., Jefferies, E., & Olivier, P. (2011). N-backer: An auditory n-back task with automatic scoring of spoken responses. Behavior research methods, 43, 888-896. [https://doi.org/10.3758/s13428-011-0074-z](https://doi.org/10.3758/s13428-011-0074-z)


## About

N-back is a secondary task that loads working memory. The numeric n-back task requires the volunteer  to listen to a stream of presented digits and to say the digit they heard 'n-steps' back in time. (e.g. in the 2-back sequence `3` `1` `6` `9`, one would be asked to say `3` on hearing `6`, `1` on hearing `9` and so on).

N-Backer uses speech synthesis and recognition to automate the presentation and scoring of this task.  The tool aims to simulate in normal volunteers some of the symptoms observed in, for example, patients in the early stages of dementia when they carry out multi-step tasks.


## Quick start

* On a computer running *Windows*, download the executable file archive (`bin/nback.zip`) and open the `.zip` archive.

* Copy the executables out of the archive to a folder.

* Run `nback-xp.exe` or `nback-inproc.exe` (see *Troubleshooting* below), and press the *Play* button.

* Read into the computer's microphone the digit you heard *1-back* (e.g. in the sequence `3` `1` `6` `9`, say `3` on hearing `1`, `1` on hearing `6`, and so on).


## Troubleshooting

* `nback-xp.exe` should be run on *Windows XP*
  * It won't work properly on *Windows Vista / 7 / 10 / 11*, as the speech recognition will also recognize system commands.
  * `nback-inproc.exe` is an attempt to use an in-process, non-shared recognizer that will not detect system commands and should work on later versions of *Windows* -- but this is not well tested.  * If necessary, you could try running it in a Virtual Machine running an older version of Windows.* If it doesn't start at all, ensure the *Microsoft .NET 3.5 SP1* installed:
  * http://www.microsoft.com/download/en/details.aspx?id=21

* To obtain the synthesized voices that it was designed for, install the *Microsoft Speech SDK 5.1*:
  * http://www.microsoft.com/download/en/details.aspx?id=10121 (`SpeechSDK51.exe` also available in the `depends` folder)

* It is designed for use with head-mounted microphone 
  * Check in the Speech Recognition Control Panel that the correct audio input is selected.

