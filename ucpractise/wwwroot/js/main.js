// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.




document.getElementById("reg").addEventListener("click", async () => {

    const login = document.getElementById("login").value;
    const pass = document.getElementById("passwor").value;
    const cp = document.getElementById("confpassword").value;
    const err = document.getElementById("err");
    if (cp != pass)
        // await reg(login, pass);
        err.style.visibility = "visible";
   // else
        
    reset();
});