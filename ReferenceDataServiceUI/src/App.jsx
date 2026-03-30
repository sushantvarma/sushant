import { useState, useRef } from 'react'
import axios from 'axios'
import './App.css'
import LoadingSpinner from './components/LoadingSpinner'

function App() {
  const [isLoading, setIsLoading] = useState(false)
  const [data, setData] = useState(null)
  const [selectedValue, setSelectedValue] = useState('')
  const [executionTime, setExecutionTime] = useState(null)
  const [error, setError] = useState(null)
  const [timingMetrics, setTimingMetrics] = useState(null)
  
  const apiStartTimeRef = useRef(null)
  const uiRenderStartRef = useRef(null)

  const API_URL = 'https://localhost:5001/api/reference-data'

  const loadReferenceData = async () => {
    setIsLoading(true)
    setError(null)
    setData(null)
    setSelectedValue('')
    setExecutionTime(null)
    setTimingMetrics(null)

    // Start overall timer
    const overallStartTime = performance.now()
    apiStartTimeRef.current = performance.now()

    try {
      const correlationId = `ui-request-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`
      
      const response = await axios.get(API_URL, {
        headers: {
          'Accept': 'application/json',
          'X-Correlation-ID': correlationId
        },
        httpsAgent: {
          rejectUnauthorized: false // Allow self-signed certificates in development
        }
      })

      // Measure API response time
      const apiResponseTime = performance.now() - apiStartTimeRef.current
      const backendExecutionTime = response.data.executionTimeMs

      // Mark UI render start
      uiRenderStartRef.current = performance.now()
      
      // Set data (triggers render)
      setData(response.data.data)
      setExecutionTime(backendExecutionTime)

      // Measure UI render time
      const uiRenderTime = performance.now() - uiRenderStartRef.current
      
      // Calculate total end-to-end time
      const totalEndToEndTime = performance.now() - overallStartTime

      // Store all timing metrics
      setTimingMetrics({
        apiExecutionTime: backendExecutionTime, // Backend processing time
        networkRoundTripTime: Math.round(apiResponseTime), // Network + API response
        uiRenderTime: Math.round(uiRenderTime), // React render time
        totalEndToEndTime: Math.round(totalEndToEndTime) // Total time
      })
    } catch (err) {
      const errorMessage = err.response?.data?.error || err.message
      setError(`Failed to load reference data: ${errorMessage}`)
      console.error('API Error:', err)
    } finally {
      setIsLoading(false)
    }
  }

  const extractDropdownOptions = () => {
    if (!data) return []

    // Handle array of items
    if (Array.isArray(data)) {
      return data
    }

    // Handle object with items property
    if (data.items && Array.isArray(data.items)) {
      return data.items
    }

    // Handle other object structures
    if (typeof data === 'object') {
      const values = Object.values(data)
      if (Array.isArray(values[0])) {
        return values[0]
      }
      return [data]
    }

    return []
  }

  const options = extractDropdownOptions()

  return (
    <div className="app-container">
      <header className="app-header">
        <h1>Reference Data Service</h1>
        <p className="subtitle">On-Demand Snowflake Data Retrieval</p>
      </header>

      <main className="app-main">
        <div className="card">
          <div className="card-content">
            <h2>Load Reference Data</h2>
            <p className="card-description">
              Click the button below to fetch reference data from Snowflake API
            </p>

            <div className="button-container">
              <button
                onClick={loadReferenceData}
                disabled={isLoading}
                className="load-button"
              >
                {isLoading ? 'Loading...' : 'Load Reference Data'}
              </button>
            </div>

            {isLoading && (
              <div className="loading-section">
                <LoadingSpinner />
                <p className="loading-text">Fetching data from Snowflake...</p>
              </div>
            )}

            {error && (
              <div className="error-section">
                <p className="error-message">❌ {error}</p>
              </div>
            )}

            {timingMetrics && (
              <div className="metrics-section">
                <h3>⏱️ Performance Metrics</h3>
                <div className="metrics-grid">
                  <div className="metric-card">
                    <div className="metric-label">Backend Execution Time</div>
                    <div className="metric-value">{timingMetrics.apiExecutionTime}ms</div>
                    <div className="metric-detail">Snowflake API processing</div>
                  </div>
                  
                  <div className="metric-card">
                    <div className="metric-label">Network Round-Trip Time</div>
                    <div className="metric-value">{timingMetrics.networkRoundTripTime}ms</div>
                    <div className="metric-detail">Frontend to backend</div>
                  </div>
                  
                  <div className="metric-card">
                    <div className="metric-label">UI Render Time</div>
                    <div className="metric-value">{timingMetrics.uiRenderTime}ms</div>
                    <div className="metric-detail">React component render</div>
                  </div>
                  
                  <div className="metric-card metric-card-total">
                    <div className="metric-label">Total End-to-End Time</div>
                    <div className="metric-value metric-value-total">{timingMetrics.totalEndToEndTime}ms</div>
                    <div className="metric-detail">Complete operation</div>
                  </div>
                </div>

                <div className="metrics-breakdown">
                  <h4>Timing Breakdown</h4>
                  <div className="timeline">
                    <div className="timeline-item">
                      <span className="timeline-label">Network RTT:</span>
                      <div className="timeline-bar" style={{width: `${(timingMetrics.networkRoundTripTime / timingMetrics.totalEndToEndTime) * 100}%`, backgroundColor: '#2196F3'}}></div>
                      <span className="timeline-time">{timingMetrics.networkRoundTripTime}ms</span>
                    </div>
                    <div className="timeline-item">
                      <span className="timeline-label">UI Render:</span>
                      <div className="timeline-bar" style={{width: `${(timingMetrics.uiRenderTime / timingMetrics.totalEndToEndTime) * 100}%`, backgroundColor: '#4CAF50'}}></div>
                      <span className="timeline-time">{timingMetrics.uiRenderTime}ms</span>
                    </div>
                  </div>
                </div>
              </div>
            )}

            {data && options.length > 0 && (
              <div className="dropdown-section">
                <label htmlFor="reference-select">
                  <strong>Select Reference Data:</strong>
                </label>
                <select
                  id="reference-select"
                  value={selectedValue}
                  onChange={(e) => setSelectedValue(e.target.value)}
                  className="dropdown"
                >
                  <option value="">-- Choose an option --</option>
                  {options.map((item, index) => (
                    <option key={index} value={JSON.stringify(item)}>
                      {typeof item === 'object'
                        ? item.name || item.id || JSON.stringify(item).substring(0, 50)
                        : item}
                    </option>
                  ))}
                </select>

                {selectedValue && (
                  <div className="selected-data">
                    <h3>Selected Item:</h3>
                    <pre className="data-display">
                      {JSON.stringify(JSON.parse(selectedValue), null, 2)}
                    </pre>
                  </div>
                )}
              </div>
            )}

            {data && options.length === 0 && (
              <div className="data-section">
                <h3>Retrieved Data:</h3>
                <pre className="data-display">
                  {JSON.stringify(data, null, 2)}
                </pre>
              </div>
            )}

            {data && (
              <div className="info-section">
                <p className="info-text">
                  ✓ Successfully loaded <strong>{options.length}</strong> items
                </p>
              </div>
            )}
          </div>
        </div>

        <aside className="info-panel">
          <div className="info-card">
            <h3>📊 How It Works</h3>
            <ol>
              <li>Click "Load Reference Data" button</li>
              <li>API calls Snowflake with your correlation ID</li>
              <li>Data is populated into the dropdown</li>
              <li>Select items to view details</li>
              <li>View performance metrics</li>
            </ol>
          </div>

          <div className="info-card">
            <h3>🔧 Configuration</h3>
            <p>
              <strong>API URL:</strong><br />
              <code>{API_URL}</code>
            </p>
            <p>
              <strong>Environment:</strong><br />
              Development (Self-signed SSL allowed)
            </p>
          </div>

          <div className="info-card">
            <h3>📈 Performance Insights</h3>
            <p>
              Three key metrics are now tracked:
            </p>
            <ul>
              <li><strong>Backend Time:</strong> Snowflake API processing</li>
              <li><strong>Network RTT:</strong> Frontend to backend</li>
              <li><strong>UI Render:</strong> React component rendering</li>
            </ul>
            <p>
              Total = all times combined
            </p>
          </div>

          <div className="info-card">
            <h3>🎯 Performance Target</h3>
            <p>
              Optimize for:
            </p>
            <ul>
              <li>Backend ≈ 200-500ms</li>
              <li>Network ≈ 50-200ms</li>
              <li>UI Render ≈ 10-50ms</li>
              <li>Total ≈ 300-700ms</li>
            </ul>
          </div>
        </aside>
      </main>

      <footer className="app-footer">
        <p>Reference Data Service UI - React + Vite</p>
        <p>POC for Snowflake Data Retrieval Optimization</p>
      </footer>
    </div>
  )
}

export default App
