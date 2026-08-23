import axios from 'axios'

export const httpClient = axios.create({
  // O Vite Proxy se encarregará de rotear '/api' para 'http://localhost:5286'
  baseURL: '/api', 
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json',
  },
})

// Interceptor para não esconder erros padronizados do ASP.NET Core (ProblemDetails)
httpClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response && error.response.data) {
      console.error('API Error Response:', error.response.data)
    } else {
      console.error('Network Error:', error.message)
    }
    return Promise.reject(error)
  }
)