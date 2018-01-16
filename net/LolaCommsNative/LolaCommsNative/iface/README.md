# am2b-iface

Central interface for communication between am2b projects. Use this as a [git submodule](https://git-scm.com/docs/git-submodule) in your project to reference the included files.

## How to Use:

### Setup

First, create a git sub-module pointing at this project:

```bash
    git submodule add https://gitlab.lrz.de/AMCode/am2b-iface.git <dest_dir>
    (replace <dest_dir> with a directory within your repository where you want the iface code to live, e.g. '/src/iface/')
```

This will clone the `iface` project into your local repository, and you must first commit and push to ensure other users of your project get the updates.

When cloning a project with submodules, it's simplest to use `git clone --recursive`, which will clone the base repository AND any existing submodules. If you have an existing repository, or clone without `--recursive`, then you can manually pull the submodules into your repository by running the command:

```
    git submodule update --init --recursive
```

The `iface` project only includes headers, which don't need to be built, but you may need to update your CMakeLists.txt to add the location of your iface submodule to your include directories, e.g. `include_directories("./src/iface/")`

### Getting Updates

Git submodules point at *specific commits* in the submodule project, and which commit is tracked separately in each repo referencing the submodule. This means that when something changes in am2b-iface, your projects will not automatically update to the latest commit when you do `git pull` from your repository (which could be a good thing if the changes made in `iface` will require you to make updates in your main project to remain compatible). To update your submodules, you can run `git pull` from the directory your submodule lives in, then when running `git status` in your main repository you should see that the submodule has changed, and you can make a commit updating to the current changeset.

```bash
    ~/am2b$ cd ./src/iface
    ~/am2b/src/iface$ git pull
    ~/am2b/src/iface$ cd ..
    ~/am2b/src$ git add iface
    ~/am2b/src$ git commit -m "Update iface to latest changes"
```
