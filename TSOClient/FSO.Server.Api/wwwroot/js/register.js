function err(form, message, success)
{
    if (form.getElementsByClassName('error').length > 0)
    {
        form.getElementsByClassName('error')[0].remove();
    }
    if (message != '')
    {
        var logger = document.createElement('p');
        logger.innerText = message;
        logger.classList.add('error', 'w-full', 'border-3', 'rounded-md', 'px-1');
        if (success)
        {
            logger.classList.add('bg-green-400', 'border-green-800');
        }
        else
        {
            logger.classList.add('bg-red-400', 'border-red-800');
        }
        form.insertBefore(logger, form.firstChild);
    }
}

const regForm = document.getElementById('register');
regForm.addEventListener('submit', (e) => 
{
    err(regForm, '', false);
    e.preventDefault();

    var username = regForm.getElementsByClassName('username')[0];
    var email = regForm.getElementsByClassName('email')[0];
    var password = regForm.getElementsByClassName('password')[0];
    var confirm = regForm.getElementsByClassName('confirm')[0];
    var emailText = 'noreply@127.0.0.1';

    if (username.value == '' || password.value == '' || confirm.value == '' || (email && email.value == ''))
    {
        password.value = '';
        confirm.value = ''
        err(regForm, 'Not all fields filled!', false);
        return;
    }
    else if (password.value != confirm.value)
    {
        password.value = '';
        confirm.value = ''
        err(regForm, 'Passwords do not match!', false);
        return;
    }
    if (email)
    {
        emailText = email.value;
    }
    
    var formData = new FormData();
    formData.append('username', username.value);
    formData.append('email', emailText);
    formData.append('password', password.value);

    fetch('/userapi/registration',
    {
        method: 'POST',
        body: formData
    })
        .then(value =>
        {
          value.json()
            .then(json => {
                if (json['error'])
                {
                    console.log(json['error'] + ': ' + json['error_description'])
                    err(regForm, 'An error occured: ' + json['error_description'], false);
                    password.value = '';
                    confirm.value = ''
                    return;
                }
                else
                {
                    if (email)
                    {
                        err(regForm, 'Verification email sent!', true);
                    }
                    else
                    {
                        err(regForm, 'Account created!', true);
                    }
                    password.value = '';
                    confirm.value = ''
                    return;
                }
            })
            .catch(reason => {
                console.log('value.json catch:', reason)
                err(regForm, 'Unknown error occured!', false);
                password.value = '';
                confirm.value = ''
                return;
            });
        })
        .catch(reason => {
            console.log('fetch catch:', reason)
            err(regForm, 'Cannot contact server!', false);
            password.value = '';
            confirm.value = ''
            return;
        });
});

const regChange = document.getElementById('change');
regChange.addEventListener('submit', (e) => 
{
    err(regChange, '', false);
    e.preventDefault();

    var username = regChange.getElementsByClassName('username')[0];
    var old = regChange.getElementsByClassName('old')[0];
    var password = regChange.getElementsByClassName('password')[0];
    var confirm = regChange.getElementsByClassName('confirm')[0];

    if (username.value == '' || old.value == '' || password.value == '' || confirm.value == '')
    {
        old.value = '';
        password.value = '';
        confirm.value = ''
        err(regChange, 'Not all fields filled!', false);
        return;
    }
    else if (password.value != confirm.value)
    {
        old.value = '';
        password.value = '';
        confirm.value = ''
        err(regChange, 'Passwords do not match!', false);
        return;
    }

    fetch('/userapi/password',
    {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            username: username.value,
            old_password: old.value,
            new_password: password.value
        })
    })
        .then(value =>
        {
          value.json()
            .then(json => {
                if (json['error'])
                {
                    console.log(json['error'] + ': ' + json['error_description'])
                    err(regChange, 'An error occured: ' + json['error_description'], false);
                    old.value = '';
                    password.value = '';
                    confirm.value = ''
                    return;
                }
                else
                {
                    err(regChange, 'Password changed!', true);
                    old.value = '';
                    password.value = '';
                    confirm.value = ''
                    return;
                }
            })
            .catch(reason => {
                console.log('value.json catch:', reason)
                err(regChange, 'Unknown error occured!', false);
                old.value = '';
                password.value = '';
                confirm.value = ''
                return;
            });
        })
        .catch(reason => {
            console.log('fetch catch:', reason)
            err(regChange, 'Cannot contact server!', false);
            old.value = '';
            password.value = '';
            confirm.value = ''
            return;
        });
});