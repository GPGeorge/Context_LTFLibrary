        window.performLogin = async (url, loginData, redirectPath) => {
            try {
                const response = await fetch(url, {
        method: 'POST',
    headers: {
        'Content-Type': 'application/json',
    'Accept': 'application/json'
                    },
    body: JSON.stringify(loginData),
    credentials: 'include'  // Ensures cookies are sent/received
                });

    const result = await response.json();

    if (response.ok && result.success) {
        console.log('Login successful, redirecting...');
    window.location.replace(redirectPath);
                } else {
        console.error('Login failed:', result.message);
    alert(result.message || 'Login failed');
                }
            } catch (error) {
        console.error('Fetch error:', error);
    alert('An error occurred during login');
            }
        };

        window.performLogout = async (url, redirectPath) => {
            try {
                const response = await fetch(url, {
        method: 'POST',
    headers: {'Content-Type': 'application/json' },
    credentials: 'include'
                });

    if (response.ok) {
        console.log('Logout successful, redirecting...');
    window.location.replace(redirectPath);
                } else {
        console.error('Logout failed');
    alert('Logout failed');
                }
            } catch (error) {
        console.error('Fetch error:', error);
    alert('An error occurred during logout');
            }
        };
