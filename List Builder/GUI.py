import os
import sys
import subprocess
from tkinter import filedialog, ttk, Tk, StringVar, Entry, Text, Checkbutton, Button, LabelFrame, Frame, END

# Define parent/root window
root = Tk()
root.title('XEB+ neutrino List Builder GUI')

# Set window size
window_width = 480
window_height = 250
screen_width = root.winfo_screenwidth()
screen_height = root.winfo_screenheight()
center_y = int(screen_width / 2 - window_width / 2)
center_x = int(screen_height / 2 - window_height / 2)

#root.geometry('%dx%d+%d+%d' % (window_width, window_height, center_x, center_y))

root.geometry(f'{window_width}x{window_height}+{center_x}+{center_y}')
root.minsize(600, 400)
root.maxsize(768, 800)

# Set window columns and rows
root.grid_columnconfigure(1, weight=1)
root.grid_rowconfigure(1, weight=1)

# Define global variables
selected = StringVar()
vmc = StringVar()
folder_path_1 = ""
folder_path_2 = ""

def enable_build():
    if selected.get() == '-u':
        directory_button2["state"] = "normal"
        ent2["state"] = "normal"
        ent2.delete(0, END)
        ent2.insert(END, ent1.get())
        directory_button2["state"] = "disabled"
        ent2["state"] = "disabled"
    else:
        directory_button2["state"] = "normal"
        ent2["state"] = "normal"

    if selected.get() and ent1.get() and ent2.get():
        build_button["state"] = "normal"

# Functions for finding folders
def current_folder_1():
    global folder_path_1
    folder_path_1 = filedialog.askdirectory(title='Choose a Directory')
    ent1.delete(0, END)
    ent1.insert(END, folder_path_1)
    enable_build()

def current_folder_2():
    global folder_path_2
    folder_path_2 = filedialog.askdirectory(title='Choose a Directory')
    ent2.delete(0, END)
    ent2.insert(END, folder_path_2)
    enable_build()

def build_list():
    global text
    process = subprocess.Popen(
        f'"{sys.executable}" -u "{os.getcwd()}/list_builder.py" "{selected.get()}" "{ent1.get()}" "{ent2.get()}" {vmc.get()}',
        stdout=subprocess.PIPE,
        universal_newlines=True,
        shell=True
    )
    lines = 0
    for stdout_line in iter(process.stdout.readline, ""):
        if lines > 2:
            text.insert(END, stdout_line)
            text.see("end")
            text.update_idletasks()
            root.update_idletasks()
            root.update()
        else:
            lines += 1
    process.stdout.close()

# Define radio buttons
radioFrame = LabelFrame(root, text=' Drive Type ')
radioFrame.pack(fill='none', pady=10)

r1 = ttk.Radiobutton(radioFrame, text='PS2 HDD', value='-h', variable=selected, command=enable_build)
r2 = ttk.Radiobutton(radioFrame, text='MX4SIO', value='-m', variable=selected, command=enable_build)
r3 = ttk.Radiobutton(radioFrame, text='MMCE', value='-c', variable=selected, command=enable_build)
r4 = ttk.Radiobutton(radioFrame, text='USB Drive', value='-u', variable=selected, command=enable_build)
r5 = ttk.Radiobutton(radioFrame, text='iLink', value='-i', variable=selected, command=enable_build)
r1.grid(padx=5, pady=5, row=0, column=1)
r2.grid(padx=5, pady=5, row=0, column=2)
r3.grid(padx=5, pady=5, row=0, column=3)
r4.grid(padx=5, pady=5, row=0, column=4)
r5.grid(padx=5, pady=5, row=0, column=5)

# Define directory and run buttons
directoryFrame1 = LabelFrame(root, text=' Games Location ')
directoryFrame1.pack(fill='none', pady=10)
directoryFrame2 = LabelFrame(root, text=' XEBPLUS Location ')
directoryFrame2.pack(fill='none', pady=10)

directory_button1 = Button(directoryFrame1, text="Choose Directory", command=current_folder_1)
directory_button1.grid(padx=5)
ent1 = Entry(directoryFrame1, font=40, width=40)
ent1.grid(row=0, column=2, padx=5)

directory_button2 = Button(directoryFrame2, text="Choose Directory", command=current_folder_2)
directory_button2.grid(padx=5)
ent2 = Entry(directoryFrame2, font=40, width=40)
ent2.grid(row=0, column=2, padx=5)

# Define VMC checkbox and build button
buttonFrame = Frame(root)
buttonFrame.pack(fill='x')

vmc_check = Checkbutton(buttonFrame, text='Create VMCs', variable=vmc, onvalue='-vmc', offvalue='')
vmc_check.grid(padx=30, row=0, column=1)

build_button = Button(buttonFrame, text="Build List", command=build_list)
build_button.grid(padx=305, pady=5, row=0, column=2)
build_button["state"] = "disabled"

text = Text(root)
text.pack()

root.update_idletasks()
root.mainloop()
